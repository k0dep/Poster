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
            messageBinder.Bind<MessageClientLeave>(ClientLeave);
            messageBinder.Bind<MessageMessage>(RecvMessage);
        }

        public void Listen(int port)
        {
            Server.Start(new IPEndPoint(0, port));
        }

        public void Stop()
        {
            Server.Stop();
        }



        private void ClientLeave(MessageClientLeave message)
        {
            var sender = MessageSenderAccess.GetSender(message);
            var (_, nick) = Clients.FirstOrDefault(t => t.Item1 == sender);

            Clients.RemoveAll(t => t.Item1 == sender);

            foreach (var (client, name) in Clients)
                client.Send(new MessageServerLeave() {Nick = nick});
        }

        private void ClientConnect(MessageClientConnect message)
        {
            foreach (var (client, name) in Clients)
                client.Send(message);

            Clients.Add((MessageSenderAccess.GetSender(message), message.Nick));
        }

        private void RecvMessage(MessageMessage message)
        {
            var sender = MessageSenderAccess.GetSender(message);
            var (_, nick) = Clients.FirstOrDefault(t => t.Item1 == sender);

            foreach (var (bc_sender, _) in Clients)
                bc_sender.Send(new MessageToClientMessage()
                {
                    Nick = nick,
                    Message = message.Message
                });
        }
    }
}