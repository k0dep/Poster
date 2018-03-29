using System.Collections.Generic;
using System.Linq;



namespace Poster
{
    public class MessageBinder : IMessageBinder, IMessageBindAccess
    {
        private Dictionary<string, List<MessageBindInstance>> _binds;


        public MessageBinder()
        {
            _binds = new Dictionary<string, List<MessageBindInstance>>();
        }



        public void Bind<TMessage>(string name, MessageBindDelegate<TMessage> onRecv)
        {
            if (!_binds.ContainsKey(name))            
                _binds[name] = new List<MessageBindInstance>();

            _binds[name].Add(new MessageBindInstance(message => onRecv((TMessage)message), typeof(TMessage)));
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


        public IList<MessageBindInstance> GetBinds(string messageName)
        {
            return _binds[messageName];
        }


        public bool AnyBind(string messageBinder)
        {
            return _binds.ContainsKey(messageBinder);
        }
    }
}