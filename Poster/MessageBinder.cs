using System.Collections.Generic;
using System.Linq;



namespace Poster
{
    public class MessageBinder : IMessageBinder, IMessageBindAccess
    {
        private Dictionary<string, List<MessageBindDelegate<object>>> _binds;



        public MessageBinder()
        {
            _binds = new Dictionary<string, List<MessageBindDelegate<object>>>();
        }



        public void Bind<TMessage>(string name, MessageBindDelegate<TMessage> onRecv)
        {
            if (!_binds.ContainsKey(name))            
                _binds[name] = new List<MessageBindDelegate<object>>();

            _binds[name].Add(message => onRecv((TMessage)message));
        }


        public void Bind<TMessage>(MessageBindDelegate<TMessage> onRecv)
        {
            var messageType = typeof(TMessage);
            var msgName = messageType.FullName;

            var nameAttributes = messageType.GetCustomAttributes(typeof(MessageAttribute), false);
            if (nameAttributes.Any())
                msgName = ((MessageAttribute) nameAttributes[0]).MessageName;

            Bind(msgName, onRecv);
        }


        public IList<MessageBindDelegate<object>> GetBinds(string messageName)
        {
            return _binds[messageName];
        }


        public bool AnyBind(string messageBinder)
        {
            return _binds.ContainsKey(messageBinder);
        }
    }
}