using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Poster.MessageBroker.Messages;
using Signals;

namespace Poster.MessageBroker
{
    public class BrokerClient : IBrokerClient
    {
        public int Timeout { get; set; }   
        
        private readonly IMessageSender _serverSender;
        private readonly ISignal<string> _messageRecvSignal;
        private readonly ISignal<IRequest> _requestSignal;

        private Random _random;
        private readonly IDictionary<int, ResponseWaiter> _responseWaiters;

        public BrokerClient(IMessageSender serverSender, IMessageBinder serverBinder, ISignal<string> messageRecvSignal, ISignal<IRequest> requestSignal)
        {
            Timeout = 3000;
            _random = new Random();
            _responseWaiters = new Dictionary<int, ResponseWaiter>();
            
            _serverSender = serverSender;
            _messageRecvSignal = messageRecvSignal;
            _requestSignal = requestSignal;

            serverBinder.Bind<BroadcastMessage>(OnRecv);

            StartSession();
        }

        private void StartSession()
        {
            _serverSender.Send(new ClientSynMessage()
            {
                ClientName =
                    $"{Environment.MachineName}/{Environment.UserName}/{Environment.UserDomainName}" +
                    $"/{Thread.CurrentThread.Name}/{Thread.CurrentThread.ManagedThreadId}/{Guid.NewGuid().ToString()}",
            });
        }
        
        private void Response(int correlationId, string message)
        {
            _serverSender.Send(new BroadcastMessage()
            {
                CorrelationId = correlationId,
                Data = message
            });
        }

        public void PublishBroadcast(string message)
        {
            _serverSender.Send(new BroadcastMessage()
            {
                CorrelationId = int.MinValue,
                Data = message
            });
        }

        public void SendBroadcast(string message, Action<string> onResponse, Action onTimeout)
        {
            var requestMessage = new BroadcastMessage()
            {
                CorrelationId = _random.Next(int.MinValue + 1, int.MaxValue - 1),
                Data = message
            }; 
            _serverSender.Send(requestMessage);

            var responseWaiter = new ResponseWaiter()
            {
                CorrelationId = requestMessage.CorrelationId,
                OnResponse = onResponse,
                OnTimeout = onTimeout,
            };
            _responseWaiters[requestMessage.CorrelationId] = responseWaiter;

            Task.Run(async () =>
            {
                await Task.Delay(Timeout);
                if (!_responseWaiters.ContainsKey(responseWaiter.CorrelationId))
                {
                    return;
                }
                
                _responseWaiters.Remove(responseWaiter.CorrelationId);
                responseWaiter.OnTimeout?.Invoke();
            });
        }
        
        private void OnRecv(BroadcastMessage message)
        {
            if (message.CorrelationId != int.MinValue)
            {
                if (!_responseWaiters.ContainsKey(message.CorrelationId))
                {
                    _requestSignal?.Invoke(new Request(message.Data, message.CorrelationId, this));
                }
                else
                {
                    var responseWaiter = _responseWaiters[message.CorrelationId];
                    _responseWaiters.Remove(message.CorrelationId);
                    responseWaiter.OnResponse.Invoke(message.Data);
                }
            }
            else
            {
                _messageRecvSignal.Invoke(message.Data);
            }
        }

        class ResponseWaiter
        {
            public int CorrelationId;
            public Action<string> OnResponse;
            public Action OnTimeout;
        }

        public class Request : IRequest
        {
            private readonly int _correlationId;
            private readonly BrokerClient _client;

            public string Data { get; }
            
            
            public Request(string data, int correlationId, BrokerClient client)
            {
                Data = data;
                _correlationId = correlationId;
                _client = client;
            }
            
            public void Response(string data)
            {
                _client.Response(_correlationId, data);
            }
        }
    }

    public interface IRequest
    {
        string Data { get; }

        void Response(string data);
    }
}