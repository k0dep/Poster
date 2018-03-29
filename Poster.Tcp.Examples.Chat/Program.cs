using Poster.Freud;
using Signals;
using System;
using System.Net;
using System.Net.Sockets;

namespace Poster.Tcp.Examples.Chat
{
    class Program
    {
        public static ISerializationProvider serializer = new FreudSerializationProvider();

        static void Main(string[] args)
        {
            if (args.Length == 1)
                StartServer(int.Parse(args[0]));
            else if(args.Length == 2)
            {
                var serverIp = new IPEndPoint(IPAddress.Parse(args[0]), int.Parse(args[1]));
                var chat = StartClientFactory(serverIp);

                ProcessChat(chat);
            }
        }

        private static void ProcessChat(ChatClient chat)
        {
            Console.Write("write your name: ");
            var nick = Console.ReadLine();

            chat.ClientConnectSignal.Listen(message =>
            {
                Console.WriteLine($"[SERVER]: connected user with nick {message}");
            });

            chat.ClientLeaveSignal.Listen(message =>
            {
                Console.WriteLine($"[SERVER]: user with nick {message} was leave");
            });

            chat.MessageRecvSignal.Listen(msg =>
            {
                Console.WriteLine($"[{msg.Item1}]: {msg.Item2}");
            });

            chat.StartSession(nick);

            while (true)
            {
                Console.Write($"{nick} > ");
                var message = Console.ReadLine();
                if (message == ":q")
                    break;
            }
        }

        private static ChatClient StartClientFactory(IPEndPoint server)
        {
            var binder = new MessageBinder();
            var recv = new MessageReceiver(binder, serializer);
            var client = new TcpClient(server);
            var tcpClient = new TcpMessageListener(recv, client);
            var sender = new TcpMessageSender(client, serializer);
            var connectSignal = new Signal<string>();
            var leaveSignal = new Signal<string>();
            var messageRecvSignal = new Signal<(string, string)>();

            var chatClient = new ChatClient(sender, binder, messageRecvSignal, connectSignal, leaveSignal);

            return chatClient;
        }

        private static void StartServer(int parse)
        {
            var binder = new MessageBinder();            
            var recv = new MessageReceiver(binder, serializer);
            var connectSignal = new Signal<IMessageSender>();
            var messageServer = new TcpMessageServer(recv, connectSignal, serializer);
            var messageSernderAccess = new LoggedMessageSenderAccess(recv);

            var chatServer = new ChatServer(binder, messageSernderAccess, messageServer);
            chatServer.Listen(parse);

            Console.ReadKey();

            chatServer.Stop();
        }

    }

    class ChatClient
    {
        public IMessageSender ServerSender { get; set; }
        public IMessageBinder ServerBinder { get; set; }

        public ISignal<(string, string)> MessageRecvSignal { get; set; }
        public ISignal<string> ClientConnectSignal { get; set; }
        public ISignal<string> ClientLeaveSignal { get; set; }

        public ChatClient(IMessageSender serverSender, IMessageBinder serverBinder, ISignal<(string, string)> messageRecvSignal, ISignal<string> clientConnectSignal, ISignal<string> clientLeaveSignal)
        {
            ServerSender = serverSender;
            ServerBinder = serverBinder;
            MessageRecvSignal = messageRecvSignal;
            ClientConnectSignal = clientConnectSignal;
            ClientLeaveSignal = clientLeaveSignal;

            ServerBinder.Bind<MessageClientConnect>(clientConnected);
            ServerBinder.Bind<MessageServerLeave>(clientLeave);
            ServerBinder.Bind<MessageToClientMessage>(recvMessage);
        }


        public void StartSession(string nick)
        {
            ServerSender.Send(new MessageClientConnect(){Nick = nick});
        }

        public void SendMessage(string message)
        {
            ServerSender.Send(new MessageMessage(){Message = message});
        }



        private void recvMessage(MessageToClientMessage message)
        {
            MessageRecvSignal.Invoke((message.Nick, message.Message));
        }

        private void clientLeave(MessageServerLeave message)
        {
            ClientLeaveSignal.Invoke(message.Nick);
        }

        private void clientConnected(MessageClientConnect message)
        {
            ClientConnectSignal.Invoke(message.Nick);
        }
    }




    class MessageClientConnect
    {
        public string Nick;
    }

    class MessageClientLeave
    {
    }

    class MessageMessage
    {
        public string Message;
    }

    class MessageToClientMessage
    {
        public string Nick;
        public string Message;
    }

    class MessageServerLeave
    {
        public string Nick;
    }
}

