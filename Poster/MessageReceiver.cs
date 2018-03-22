using System;
using System.Collections.Generic;

namespace Poster
{
    [Serializable]
    public class MessageNotBindedException : Exception
    {
        public string MessageName;
        public object MessageObject;

        public MessageNotBindedException(string message, string msgName, object msgObject) : base(message)
        {
            MessageName = msgName;
            MessageObject = msgObject;
        }
    }

    public class MessageReceiver : IMessageReceiver, IMessageRecieverHistory
    {
        public IMessageBindAccess MessageBindAccess { get; private set; }
        public IList<MessageLogItem> MessageHistory { get; private set; }


        public MessageReceiver(IMessageBindAccess bindAccess, int historyLen = 100)
        {
            MessageBindAccess = bindAccess;
            MessageHistory = new CycleList<MessageLogItem>(historyLen);
        }


        public void Receive(string name, object message, object custom)
        {
            MessageHistory.Add(new MessageLogItem(DateTime.UtcNow, name, message, custom));

            if (!MessageBindAccess.AnyBind(name))
                throw new MessageNotBindedException("Received not binded message: " + name, name, message);

            var binds = MessageBindAccess.GetBinds(name);

            for (int i = 0; i < binds.Count; i++)
                binds[i].Invoke(message);
        }
    }
}