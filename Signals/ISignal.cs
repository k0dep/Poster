namespace Signals
{
    public interface ISignal
    {
        void Listen(EmptySignalDelegate listener);
        void Unlisten(EmptySignalDelegate listener);
        void Invoke();
    }

    public interface ISignal<TMessage>
    {
        void Listen(SignalDelegate<TMessage> listener);
        void Unlisten(SignalDelegate<TMessage> listener);
        void Invoke(TMessage msg);
    }
}