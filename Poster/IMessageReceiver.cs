namespace Poster
{
    public interface IMessageReceiver
    {
        void Receive(string name, byte[] message, object custom);
    }
}
