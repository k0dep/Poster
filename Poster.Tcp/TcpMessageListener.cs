using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Poster.Tcp
{
    public class TcpMessageListener
    {
        public const int ReadTimeout = 500;

        public IMessageReceiver Receiver { get; private set; }

        public NetworkStream NetworkStream { get; private set; }

        public object CustomData { get; set; }


        public TcpMessageListener(IMessageReceiver receiver, TcpClient client, object customData = null)
        {
            Receiver = receiver;
            NetworkStream = client.GetStream();
            CustomData = customData;
            Start();
        }

        public TcpMessageListener(IMessageReceiver receiver, NetworkStream networkStream, object customData = null)
        {
            Receiver = receiver;
            NetworkStream = networkStream;
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
                try
                {
                    NetworkStream.Read(buffer, 0, 4);
                }
                catch (IOException e)
                {
                    continue;
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