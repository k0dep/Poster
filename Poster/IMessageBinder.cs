using System.Collections.Generic;

namespace Poster
{
    public delegate void MessageBindDelegate<TMessage>(TMessage message);

    public interface IMessageBindAccess
    {
        IList<MessageBindDelegate<object>> GetBinds(string messageName);
        bool AnyBind(string messageBinder);
    }

    public interface IMessageBinder
    {
        void Bind<TMessage>(string name, MessageBindDelegate<TMessage> onRecv);
        void Bind<TMessage>(MessageBindDelegate<TMessage> onRecv);
    }

}
