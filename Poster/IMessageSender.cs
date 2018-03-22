using System;

namespace Poster
{
    public class MessageSendError
    {
    }

    public interface IMessageSender
    {
        void Send(string name, object message, Action<MessageSendError> sendError);
        void Send(string name, object message);
        void Send<TMessage>(TMessage message);
        void Send<TMessage>(TMessage message, Action<MessageSendError> sendError);
    }
}