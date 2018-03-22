using System;
using System.Collections.Generic;

namespace Poster
{
    public struct MessageLogItem
    {
        public readonly DateTime Timestamp;
        public readonly string Name;
        public readonly object Message;
        public readonly object CustomData;

        public MessageLogItem(DateTime timestamp, string name, object message, object customData)
        {
            Timestamp = timestamp;
            Name = name;
            Message = message;
            CustomData = customData;
        }
    }

    public interface IMessageRecieverHistory
    {
        IList<MessageLogItem> MessageHistory { get; }
    }
}
