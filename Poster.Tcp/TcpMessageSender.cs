using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Poster
{
    public class TcpMessageSender : IMessageSender
    {        
        private NetworkStream _stream;
        private ISerializationProvider _serializator;

        public TcpMessageSender(TcpClient client, ISerializationProvider serializator)
        {
            if (serializator == null)
                throw new ArgumentNullException("serializator");

            if (client == null)
                throw new ArgumentNullException("client");

            if (!client.Connected)
                throw new ArgumentException("tcp client should connected");

            _init(client.GetStream(), serializator);
        }

        public TcpMessageSender(NetworkStream stream, ISerializationProvider serializator)
        {
            if (serializator == null)
                throw new ArgumentNullException("serializator");

            if (stream == null)
                throw new ArgumentNullException("stream");

            if (stream.CanWrite)
                throw new ArgumentException("NetworkStream is'nt writable");

            _init(stream, serializator);
        }

        private void _init(NetworkStream stream, ISerializationProvider serializator)
        {
            _serializator = serializator;
            _stream = stream;
        }


        public void Send(string name, object message, Action<MessageSendError> sendError)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (name.Length == 0)
                throw new ArgumentException("argument is empty string", "name");

            if (message == null)
                throw new ArgumentNullException("message");

            using (var memStream = new MemoryStream())
            {
                var data = _serializator.Serialize(message);
                var nameBytes = Encoding.ASCII.GetBytes(name);

                var fullLength = 4 + nameBytes.Length + 4 + data.Length;

                memStream.Write(BitConverter.GetBytes(fullLength), 0, 4);
                memStream.Write(BitConverter.GetBytes(nameBytes.Length), 0, 4);
                memStream.Write(nameBytes, 0, nameBytes.Length);
                memStream.Write(BitConverter.GetBytes(data.Length), 0, 4);
                memStream.Write(data, 0, data.Length);

                _stream.Write(memStream.ToArray(), 0, (int)memStream.Length);
            }
        }


        public void Send(string name, object message)
        {
            Send(name, message, error => {});
        }


        public void Send<TMessage>(TMessage message)
        {
            Send(message, error => {});
        }


        public void Send<TMessage>(TMessage message, Action<MessageSendError> sendError)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            var messageType = typeof(TMessage);
            var name = messageType.FullName;

            var msgAttr = messageType.GetCustomAttributes(typeof(MessageAttribute), false);
            if (msgAttr.Length != 0)
                name = ((MessageAttribute)msgAttr[0]).MessageName;

            Send(name, message, sendError);
        }
    }
}
