using System;

namespace Poster
{
    public class MessageSenderNoSerialize : IMessageSender
    {
        private readonly IMessageReceiver _receiver;

        public MessageSenderNoSerialize(ISerializationProvider serializator, IMessageReceiver reciever)
        {
            _receiver = reciever;
        }

        public void Send(string name, object message, Action<MessageSendError> sendError)
        {
            _receiver.Receive(name, null, message);
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