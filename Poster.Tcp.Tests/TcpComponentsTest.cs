using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using Poster.Freud;
using Signals;

namespace Poster.Tcp.Tests
{
    [TestFixture]
    public class TcpComponentsTest
    {
        private int Port = 32351;
        private IMessageBinder Binder;
        private IMessageReceiver Reciever;
        private ISignal<IMessageSender> ServerConnectSignal;
        private ISerializationProvider Serialier;


        public IMessageSender CreateSender()
        {
            var client = new TcpClient("localhost", Port);
            return new TcpMessageSender(client, Serialier);
        }


        [SetUp]
        public void SetUp()
        {
            var binds = new MessageBinder();
            Binder = binds;
            Reciever = new MessageReceiver(binds);
            ServerConnectSignal = new Signal<IMessageSender>();
            Serialier = new FreudSerializationProvider();
        }

        [Test]
        public void ServerReceiveTest()
        {
            var server = new TcpMessageServer(Reciever, ServerConnectSignal, Serialier);
            server.Start(new IPEndPoint(0, Port));

            var isRecv = false;
            Binder.Bind<foo>(t => isRecv = true);

            var client = CreateSender();
            client.Send(new foo());

            server.Stop();

            Assert.True(isRecv);
        }

        class foo
        {
            public int data;
        }
    }
}
