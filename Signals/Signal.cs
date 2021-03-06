﻿using System;
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

        public void Unlisten(EmptySignalDelegate listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            _listeners.RemoveAll(t => t.Target == listener);
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


        public static Signal operator +(Signal signal, EmptySignalDelegate listener)
        {
            signal.Listen(listener);
            return signal;
        }

        public static Signal operator -(Signal signal, EmptySignalDelegate listener)
        {
            signal.Unlisten(listener);
            return signal;
        }
    }
}
