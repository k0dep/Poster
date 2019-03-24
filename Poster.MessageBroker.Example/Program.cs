using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Poster.DefaultSerializationProvider;
using Poster.Freud;
using Poster.Tcp;
using Signals;

namespace Poster.MessageBroker.Example
{
    internal class Program
    {
        public static ISerializationProvider serializer = new JsonSerializationProvider.JsonSerializationProvider();
        
        public static void Main(string[] args)
        {
            if (args[0] == "-s")
            {
                StartServer();
            }
            else
            {
                if (args[0] == "1")
                {
                    StartRequester();
                }
                else
                {
                    StartResponser();
                }
            }
        }

        private static void StartResponser()
        {
            Console.WriteLine("Responser starting");
            var binder = new MessageBinder();
            var recv = new MessageReceiver(binder, serializer);
            
            var client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Loopback, 9064));
            
            var tcpClient = new TcpMessageListener(recv, client);
            var sender = new TcpMessageSender(client, serializer);
            var messageRecvSignal = new Signal<string>();
            var messageRequestSignal = new Signal<IRequest>();

            var brokerClient = new BrokerClient(sender, binder, messageRecvSignal, messageRequestSignal);

            messageRequestSignal.Listen(request =>
            {
                request.Response(request.Data + " - ok!");
            });

            Console.ReadKey();
            
            tcpClient.Stop();
            client.Close();
        }

        private static void StartRequester()
        {
            Console.WriteLine("Requester starting");
            var binder = new MessageBinder();
            var recv = new MessageReceiver(binder, serializer);
            
            var client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Loopback, 9064));
            
            var tcpClient = new TcpMessageListener(recv, client);
            var sender = new TcpMessageSender(client, serializer);
            var messageRecvSignal = new Signal<string>();
            var messageRequestSignal = new Signal<IRequest>();

            var brokerClient = new BrokerClient(sender, binder, messageRecvSignal, messageRequestSignal);

            Send();            

            void Send()
            {
                var stopwatch = Stopwatch.StartNew();
                stopwatch.Start();
                
                brokerClient.SendBroadcast(Guid.NewGuid().ToString(), s =>
                {
                    stopwatch.Stop();
                    Console.WriteLine(stopwatch.Elapsed.TotalMilliseconds);
                    Send();
                }, () =>
                {
                    Console.WriteLine("!!!!!!TIMEOUT!!!!!");
                    Send();
                });
            }

            Console.ReadKey();
            tcpClient.Stop();
            client.Close();
        }

        private static void StartServer()
        {
            var binder = new MessageBinder();            
            var recv = new MessageReceiver(binder, serializer);
            var connectSignal = new Signal<IMessageSender>();
            var messageServer = new TcpMessageServer(recv, connectSignal, serializer);
            var messageSernderAccess = new LoggedMessageSenderAccess(recv);

            var brokerServer = new BrokerServer(binder, messageSernderAccess, messageServer); 
            brokerServer.Listen(8000);

            Console.ReadKey();

            brokerServer.Stop();
        }
    }
}