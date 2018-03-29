using System.Linq;

namespace Poster
{
    public class LoggedMessageSenderAccess : IMessageSenderAccess
    {
        public IMessageRecieverHistory ReceiverHistory { get; private set; }


        public LoggedMessageSenderAccess(IMessageRecieverHistory receiverHistory)
        {
            ReceiverHistory = receiverHistory;
        }


        public IMessageSender GetSender(object message)
        {
            var historyHit =
                ReceiverHistory.MessageHistory.FirstOrDefault(t => t.DeserializedMessages.Contains(message));

            var sender = historyHit?.CustomData as IMessageSender;

            return sender;
        }
    }
}
