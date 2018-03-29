using System;
using System.Collections.Generic;

namespace Poster
{
    public delegate void MessageBindDelegate<TMessage>(TMessage message);

    public struct MessageBindInstance
    {
        public MessageBindDelegate<object> BindAction;
        public Type MessageType;

        public MessageBindInstance(MessageBindDelegate<object> bindAction, Type messageType)
        {
            BindAction = bindAction;
            MessageType = messageType;
        }
    }

    public interface IMessageBindAccess
    {
        IList<MessageBindInstance> GetBinds(string messageName);
        bool AnyBind(string messageBinder);
    }

    public interface IMessageBinder
    {
        void Bind<TMessage>(string name, MessageBindDelegate<TMessage> onRecv);
        void Bind<TMessage>(MessageBindDelegate<TMessage> onRecv);
    }

}
