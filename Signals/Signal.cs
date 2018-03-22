using System;
using System.Collections.Generic;

namespace Signals
{
    public delegate void EmptySignalDelegate();
    public delegate void SignalDelegate<TMessage>(TMessage message);



    public class Signal : ISignal
    {
        private readonly List<WeakReference> _listeners = new List<WeakReference>();

        public void Listen(EmptySignalDelegate listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            _listeners.Add(new WeakReference(listener));
        }

        public void Invoke()
        {
            _listeners.RemoveAll(t =>
            {
                if (!t.IsAlive)
                    return true;

                return t.Target is EmptySignalDelegate target && target.Target == null;
            });

            foreach (var weakReference in _listeners)
            {
                var t = weakReference.Target as EmptySignalDelegate;
                t.Invoke();
            }
        }
    }

    public class Signal<TMessage> : ISignal<TMessage>
    {
        private readonly List<WeakReference> _listeners = new List<WeakReference>();

        public void Listen(SignalDelegate<TMessage> listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            _listeners.Add(new WeakReference(listener));
        }

        public void Invoke(TMessage message)
        {
            _listeners.RemoveAll(t =>
            {
                if (!t.IsAlive)
                    return true;

                return t.Target is SignalDelegate<TMessage> target && target.Target == null;
            });

            foreach (var weakReference in _listeners)
            {
                var t = weakReference.Target as SignalDelegate<TMessage>;
                t.Invoke(message);
            }
        }
    }
}
