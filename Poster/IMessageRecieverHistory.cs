using System;
using System.Collections.Generic;

namespace Poster
{
    public class MessageLogItem
    {
        public readonly DateTime Timestamp;
        public readonly string Name;
        public readonly byte[] Message;
        public readonly List<object> DeserializedMessages;
        public readonly object CustomData;

        public MessageLogItem(DateTime timestamp, string name, byte[] message, object customData)
        {
            Timestamp = timestamp;
            Name = name;
            Message = message;
            CustomData = customData;
            DeserializedMessages = new List<object>();
        }

        public void AddMessage(object message)
        {
            DeserializedMessages.Add(message);
        }
    }

    public interface IMessageRecieverHistory
    {
        IList<MessageLogItem> MessageHistory { get; }
    }
}
