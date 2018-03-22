namespace Poster
{
    public interface IMessageReceiver
    {
        void Receive(string name, object message, object custom);
    }
}
