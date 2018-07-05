using System;

namespace Poster
{
    public class MessageSender : IMessageSender
    {
        private readonly IMessageReceiver _receiver;
        private readonly ISerializationProvider _serializator;

        public MessageSender(ISerializationProvider serializator, IMessageReceiver reciever)
        {
            if (serializator == null)
            {
                throw new ArgumentNullException("serializator");
            }

            _receiver = reciever;
            _serializator = serializator;
        }

        public void Send(string name, object message, Action<MessageSendError> sendError)
        {
            _receiver.Receive(name, _serializator.Serialize(message), null);
        }


        public void Send(string name, object message)
        {
            Send(name, message, error => { });
        }


        public void Send<TMessage>(TMessage message)
        {
            Send(message, error => { });
        }


        public void Send<TMessage>(TMessage message, Action<MessageSendError> sendError)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            var messageType = typeof(TMessage);
            var name = messageType.FullName;

            var msgAttr = messageType.GetCustomAttributes(typeof(MessageAttribute), false);
            if (msgAttr.Length != 0)
                name = ((MessageAttribute) msgAttr[0]).MessageName;

            Send(name, message, sendError);
        }
    }
}