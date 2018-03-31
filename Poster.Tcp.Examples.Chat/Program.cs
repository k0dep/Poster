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
        private static bool leave;

        static void Main(string[] args)
        {
            if (args.Length == 1)
                StartServer(int.Parse(args[0]));
            else if(args.Length == 2)
            {
                var serverIp = new IPEndPoint(IPAddress.Parse(args[0]), int.Parse(args[1]));
                var (chat, client, listener) = StartClientFactory(serverIp);

                listener.DisconnectSignal.Listen(t => leave = true);

                ProcessChat(chat);

                listener.Stop();
                client.Close();
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

            while (true && !leave)
            {
                Console.Write($"{nick} > ");
                var message = Console.ReadLine();
                if (message == ":q")
                    break;

                if(leave)
                    return;

                chat.SendMessage(message);
            }


        }

        private static (ChatClient, TcpClient, TcpMessageListener) StartClientFactory(IPEndPoint server)
        {
            var binder = new MessageBinder();
            var recv = new MessageReceiver(binder, serializer);
            var client = new TcpClient();
            client.Connect(server);
            var tcpClient = new TcpMessageListener(recv, client);
            var sender = new TcpMessageSender(client, serializer);
            var connectSignal = new Signal<string>();
            var leaveSignal = new Signal<string>();
            var messageRecvSignal = new Signal<(string, string)>();

            var chatClient = new ChatClient(sender, binder, messageRecvSignal, connectSignal, leaveSignal);

            return (chatClient, client, tcpClient);
        }

        private static void disconnectClient(TcpMessageListener _)
        {
            
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

