using System;
using System.Collections.Generic;

namespace Signals
{
    public class Signal<TMessage> : ISignal<TMessage>
    {
        private readonly List<WeakReference> _listeners = new List<WeakReference>();

        public void Listen(SignalDelegate<TMessage> listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            _listeners.Add(new WeakReference(listener));
        }


        public void Unlisten(SignalDelegate<TMessage> listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            _listeners.RemoveAll(t => t.Target == listener);
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

        public static Signal<TMessage> operator +(Signal<TMessage> signal, SignalDelegate<TMessage> listener)
        {
            signal.Listen(listener);
            return signal;
        }

        public static Signal<TMessage> operator -(Signal<TMessage> signal, SignalDelegate<TMessage> listener)
        {
            signal.Unlisten(listener);
            return signal;
        }
    }
}