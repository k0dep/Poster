using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Poster.Tcp.Examples.Chat
{
    class ChatServer
    {
        public IMessageBinder MessageBinder { get; set; }
        public IMessageSenderAccess MessageSenderAccess { get; set; }
        public TcpMessageServer Server { get; set; }

        private List<(IMessageSender, string)> Clients;


        public ChatServer(IMessageBinder messageBinder, IMessageSenderAccess messageSenderAccess,
            TcpMessageServer server)
        {
            MessageBinder = messageBinder;
            MessageSenderAccess = messageSenderAccess;
            Server = server;

            Clients = new List<(IMessageSender, string)>();

            messageBinder.Bind<MessageClientConnect>(ClientConnect);
            messageBinder.Bind<MessageMessage>(RecvMessage);

            Server.DisconnectClientSignal.Listen(t => ClientLeave((IMessageSender) t.CustomData));
        }

        public void Listen(int port)
        {
            Server.Start(new IPEndPoint(0, port));
        }

        public void Stop()
        {
            Server.Stop();
        }



        private void ClientLeave(IMessageSender sender)
        {
            var (_, nick) = Clients.FirstOrDefault(t => t.Item1 == sender);

            Clients.RemoveAll(t => t.Item1 == sender);

            foreach (var (client, name) in Clients)
                client.Send(new MessageServerLeave() {Nick = nick});

            Console.WriteLine($"< {nick} disconnect");
        }

        private void ClientConnect(MessageClientConnect message)
        {
            foreach (var (client, name) in Clients)
                client.Send(message);

            Clients.Add((MessageSenderAccess.GetSender(message), message.Nick));

            Console.WriteLine($"> {message.Nick} connect");
        }

        private void RecvMessage(MessageMessage message)
        {
            var sender = MessageSenderAccess.GetSender(message);
            var (_, nick) = Clients.FirstOrDefault(t => t.Item1 == sender);

            foreach (var (bc_sender, _) in Clients)
            {
                if(bc_sender == sender)
                    continue;
                
                bc_sender.Send(new MessageToClientMessage()
                {
                    Nick = nick,
                    Message = message.Message
                });
            }

            Console.WriteLine($"! {nick}: {message.Message}");
        }
    }
}