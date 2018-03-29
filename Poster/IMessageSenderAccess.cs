namespace Poster
{
    public interface IMessageSenderAccess
    {
        IMessageSender GetSender(object message);
    }
}
