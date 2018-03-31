using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Signals;

namespace Poster.Tcp
{
    public class TcpMessageListener
    {
        public const int ReadTimeout = 500;

        public IMessageReceiver Receiver { get; }

        public NetworkStream NetworkStream { get; private set; }

        public ISignal<TcpMessageListener> DisconnectSignal { get; }

        public object CustomData { get; set; }


        public TcpMessageListener(IMessageReceiver receiver, TcpClient client, object customData = null)
        {
            Receiver = receiver;
            NetworkStream = client.GetStream();
            CustomData = customData;
            DisconnectSignal = new Signal<TcpMessageListener>();
            Start();
        }

        public TcpMessageListener(IMessageReceiver receiver, NetworkStream networkStream, object customData = null)
        {
            Receiver = receiver;
            NetworkStream = networkStream;
            CustomData = customData;
            DisconnectSignal = new Signal<TcpMessageListener>();
            Start();
        }

        public TcpMessageListener(IMessageReceiver receiver, NetworkStream networkStream, ISignal<TcpMessageListener> disconnectSignal, object customData)
        {
            Receiver = receiver;
            NetworkStream = networkStream;
            DisconnectSignal = disconnectSignal;
            CustomData = customData;
            Start();
        }

        public void Stop()
        {
            NetworkStream = null;
        }

        private void Start()
        {
            NetworkStream.ReadTimeout = ReadTimeout;

            var listenThread = new Thread(listen);
            listenThread.Name = "Tcp message listener";
            listenThread.Start();
        }

        private void listen()
        {
            var buffer = new byte[2048];
            while (NetworkStream != null)
            {
                int bylesLength = 0;
                try
                {
                    bylesLength = NetworkStream.Read(buffer, 0, 4);
                }
                catch (IOException e)
                {
                    continue;
                }
                catch (ObjectDisposedException e)
                {
                    continue;
                }

                if (bylesLength == 0)
                {
                    DisconnectSignal.Invoke(this);
                    break;
                }

                var length = BitConverter.ToInt32(buffer, 0);

                NetworkStream.Read(buffer, 0, length);

                using (var memStream = new MemoryStream(buffer))
                using (var binaryStream = new BinaryReader(memStream))
                {
                    var name = Encoding.ASCII.GetString(binaryStream.ReadBytes(binaryStream.ReadInt32()));
                    var data = binaryStream.ReadBytes(binaryStream.ReadInt32());

                    Receiver.Receive(name, data, CustomData);
                }
            }
        }
    }
}