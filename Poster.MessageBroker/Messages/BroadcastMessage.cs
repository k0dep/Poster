using System;

namespace Poster.MessageBroker.Messages
{
    [Serializable]
    public class BroadcastMessage
    {
        public int CorrelationId;
        public string Data;
    }
}