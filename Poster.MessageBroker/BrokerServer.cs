using System;
using System.Collections.Generic;
using System.Net;
using Poster.MessageBroker.Messages;

namespace Poster.MessageBroker
{
    /// <summary>
    ///     Message broker server implementation
    /// </summary>
    public class BrokerServer : IDisposable
    {
        public IMessageSenderAccess MessageSenderAccess { get; set; }
        public TcpMessageServer Server { get; set; }

        private readonly IDictionary<IMessageSender, string> _clients;
        private readonly Stack<IMessageSender> _errorClients;

        public BrokerServer(IMessageBinder messageBinder, IMessageSenderAccess messageSenderAccess, TcpMessageServer server)
        {
            MessageSenderAccess = messageSenderAccess;
            Server = server;
            _errorClients = new Stack<IMessageSender>();

            _clients = new Dictionary<IMessageSender, string>();

            messageBinder.Bind<ClientSynMessage>(ClientConnect);
            messageBinder.Bind<BroadcastMessage>(RecvMessage);

            Server.DisconnectClientSignal.Listen(t => ClientLeave((IMessageSender) t.CustomData));
        }

        public void Listen(int port)
        {
            Server.Start(new IPEndPoint(0, port));
        }

        public void Stop()
        {
            Server?.Stop();
        }
        
        public void Dispose()
        {
            Stop();
        }

        private void ClientLeave(IMessageSender sender)
        {
            string name;
            lock (_clients)
            {
                name = _clients[sender];
                _clients.Remove(sender);
            }

            Console.WriteLine($"[-] {name} disconnected");
        }

        private void ClientConnect(ClientSynMessage message)
        {
            lock (_clients)
            {
                _clients[MessageSenderAccess.GetSender(message)] = message.ClientName;
            }

            Console.WriteLine($"[+] {message.ClientName} connect");
        }

        private void RecvMessage(BroadcastMessage message)
        {
            var sender = MessageSenderAccess.GetSender(message);

            lock (_clients)
            {
                foreach (var bcSender in _clients)
                {
                    if (bcSender.Key == sender)
                    {
                        continue;
                    }

                    try
                    {
                        bcSender.Key.Send(message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(
                            $"[error] cant send message to client with name {bcSender.Value}. it will remove from mail list");
                        _errorClients.Push(bcSender.Key);
                    }
                }

                while (_errorClients.Count > 0)
                {
                    ClientLeave(_errorClients.Pop());
                }
            }
        }
    }
}