using System;
using Signals;

namespace Poster.MessageBroker.Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var port = args.Length == 1 ? int.Parse(args[0]) : 9064;
            
            var serializer = new JsonSerializationProvider.JsonSerializationProvider();
            var binder = new MessageBinder();            
            var recv = new MessageReceiver(binder, serializer);
            var connectSignal = new Signal<IMessageSender>();
            var messageServer = new TcpMessageServer(recv, connectSignal, serializer);
            var messageSernderAccess = new LoggedMessageSenderAccess(recv);

            var brokerServer = new BrokerServer(binder, messageSernderAccess, messageServer); 
            brokerServer.Listen(port);

            Console.WriteLine($"Start server at {port}");
            Console.WriteLine($"Write key for stop server");
            Console.ReadKey();

            brokerServer.Stop();
        }
    }
}