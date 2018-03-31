using System;
using Poster.Tcp;
using Signals;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Poster
{
    public class TcpMessageServer
    {
        public IMessageReceiver MessageReceiver { get; set; }
        public ISignal<IMessageSender> ConnectClientSignal { get; set; }
        public ISerializationProvider SerializationProvider { get; set; }
        public ISignal<TcpMessageListener> DisconnectClientSignal { get; set; }

        private TcpListener _tcpListener;
        private List<ClientInfo> _clientsList;
        private Thread _acceptClientThread;

        public TcpMessageServer(IMessageReceiver messageReceiver, ISignal<IMessageSender> connectClientSignal,
            ISerializationProvider serializationProvider, ISignal<TcpMessageListener> disconnectClientSignal)
        {
            MessageReceiver = messageReceiver;
            ConnectClientSignal = connectClientSignal;
            SerializationProvider = serializationProvider;
            DisconnectClientSignal = disconnectClientSignal;

            _clientsList = new List<ClientInfo>();
        }

        public TcpMessageServer(IMessageReceiver messageReceiver, ISignal<IMessageSender> connectClientSignal,
            ISerializationProvider serializationProvider)
        {
            MessageReceiver = messageReceiver;
            ConnectClientSignal = connectClientSignal;
            SerializationProvider = serializationProvider;
            DisconnectClientSignal = new Signal<TcpMessageListener>();

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

            var listener = new TcpMessageListener(MessageReceiver, client.GetStream(), DisconnectClientSignal, sender);
        }

        private void acceptClientThread()
        {
            while (_tcpListener != null)
            {
                var thread = new Thread(handleConnection);
                thread.Name = "ClientAccepter";
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
