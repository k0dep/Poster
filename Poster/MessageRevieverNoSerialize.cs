using System;
using System.Collections.Generic;

namespace Poster
{
    public class MessageRevieverNoSerialize : IMessageReceiver, IMessageRecieverHistory
    {
        public IMessageBindAccess MessageBindAccess { get; private set; }
        public IList<MessageLogItem> MessageHistory { get; private set; }
        
        public bool ThrowNoneBinds { get; set; }

        public MessageRevieverNoSerialize(IMessageBindAccess bindAccess, int historyLen = 100)
        {
            MessageBindAccess = bindAccess;
            MessageHistory = new CycleList<MessageLogItem>(historyLen);
        }


        public void Receive(string name, byte[] message, object custom)
        {
            var logItem = new MessageLogItem(DateTime.UtcNow, name, message, custom);
            MessageHistory.Add(logItem);

            if (!MessageBindAccess.AnyBind(name))
            {
                if (ThrowNoneBinds)
                {
                    throw new MessageNotBindedException("Received not binded message: " + name, name, message);
                }
                else
                {
                    return;
                }
            }

            var binds = MessageBindAccess.GetBinds(name);

            for (int i = 0; i < binds.Count; i++)
            {
                binds[i].BindAction(custom);
            }
        }
    }
}