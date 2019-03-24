using System;

namespace Poster.MessageBroker
{
    public interface IBrokerClient
    {
        int Timeout { get; set; }
        void PublishBroadcast(string message);
        void SendBroadcast(string message, Action<string> onResponse, Action onTimeout);
    }
}