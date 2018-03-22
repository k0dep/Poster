using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Signals;

namespace Poster
{
    public class TcpMessageServer
    {
        public IMessageReceiver MessageReceiver { get; set; }
        public ISignal<IMessageSender> ConnectClientSignal { get; set; }
        public ISerializationProvider SerializationProvider { get; set; }

        private TcpListener _tcpListener;
        private List<ClientInfo> _clientsList;
        private Thread _acceptClientThread;

        public TcpMessageServer(IMessageReceiver messageReceiver, ISignal<IMessageSender> connectClientSignal, ISerializationProvider serializationProvider)
        {
            MessageReceiver = messageReceiver;
            ConnectClientSignal = connectClientSignal;
            SerializationProvider = serializationProvider;

            _clientsList = new List<ClientInfo>();
        }

        public void Start(IPEndPoint endpoint)
        {
            if(_tcpListener != null)
                Stop();

            _tcpListener = new TcpListener(endpoint);
            _tcpListener.Start();

            _acceptClientThread = new Thread(acceptClientThread);
            _acceptClientThread.Start();
        }

        public void Stop()
        {
            if (_tcpListener != null)
            {
                lock (_tcpListener)
                {
                    _tcpListener.Stop();
                    _tcpListener = null;
                    _acceptClientThread.Join();
                }
            }
        }

        private void handleConnection(object stateInfo)
        {
            var client = stateInfo as TcpClient;
            var sender = new TcpMessageSender(client, SerializationProvider);

            _clientsList.Add(new ClientInfo()
            {
                Sender = sender,
                TcpClient = client
            });

            ConnectClientSignal.Invoke(sender);


            var buffer = new byte[2048];
            var clientStream = client.GetStream();
            while (client.Connected && _tcpListener != null)
            {
                clientStream.Read(buffer, 0, 4);
                var length = BitConverter.ToInt32(buffer, 0);

                clientStream.Read(buffer, 0, length);

                using (var memStream = new MemoryStream(buffer))
                using (var binaryStream = new BinaryReader(memStream))
                {
                    var name = Encoding.ASCII.GetString(binaryStream.ReadBytes(binaryStream.ReadInt32()));
                    var data = binaryStream.ReadBytes(binaryStream.ReadInt32());

                    MessageReceiver.Receive(name, data, (IMessageSender)sender);
                }
            }
        }

        private void acceptClientThread()
        {
            while (_tcpListener != null)
            {
                var thread = new Thread(handleConnection);
                thread.Start(_tcpListener.AcceptTcpClient());
            }
        }

        struct ClientInfo
        {
            public TcpClient TcpClient;
            public TcpMessageSender Sender;
        }
    }
}
