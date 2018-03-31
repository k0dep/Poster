using NUnit.Framework;
using Poster.Freud;
using Signals;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Poster.Tcp.Tests
{
    [TestFixture]
    public class TcpComponentsTest
    {
        private int Port = 32351;
        private ISerializationProvider Serialier = new FreudSerializationProvider();


        public IMessageSender CreateSender()
        {
            var client = new TcpClient("localhost", Port);
            return new TcpMessageSender(client, Serialier);
        }

        public (IMessageSender, IMessageBinder) CreateClientSide()
        {
            var binder = new MessageBinder();
            var recv = new MessageReceiver(binder, Serialier);
            var client = new TcpClient("localhost", Port);
            var sender = new TcpMessageSender(client, Serialier);
            var listener = new TcpMessageListener(recv, client, sender);

            return (sender, binder);
        }

        public (TcpMessageServer, IMessageBinder, ISignal<IMessageSender>) CreateServerSide()
        {
            var binds = new MessageBinder();
            var connectSignal = new Signal<IMessageSender>();
            var recv = new MessageReceiver(binds, Serialier);
            var server = new TcpMessageServer(recv, connectSignal, Serialier);

            return (server, binds, connectSignal);
        }


        [Test]
        public void ServerReceiveTest()
        {
            var (server, sBinder, _) = CreateServerSide();

            server.Start(new IPEndPoint(0, Port));

            var isRecv = false;
            var waiter = new AutoResetEvent(false);

            sBinder.Bind<string>(t =>
            {
                isRecv = true;
                waiter.Set();
            });

            var client = CreateSender();
            client.Send("hello");

            waiter.WaitOne(3000);
            waiter.Dispose();

            server.Stop();

            Assert.True(isRecv);
        }

        [Test]
        public void RequestResponseTest()
        {
            var o = TestContext.Out;

            var (server, sBinder, onConnect) = CreateServerSide();
            server.Start(new IPEndPoint(0, Port));

            IMessageSender s2cSender = null;
            onConnect.Listen(s => s2cSender = s);

            var (cSender, cBinder) = CreateClientSide();
            sBinder.Bind("reqv", (string req) => s2cSender.Send("resp", "bye"));

            var isReqv = false;

            var notifier = new AutoResetEvent(false);
            cBinder.Bind("resp", (string resp) =>
            {
                isReqv = true;
                notifier.Set();
            });

            cSender.Send("reqv", "hi");

            notifier.WaitOne(500);
            notifier.Dispose();

            server.Stop();

            Assert.True(isReqv);
        }


        [Test]
        public void ShouldDisconnectNotify()
        {
            var server = new TcpMessageServer(new MessageReceiver(new MessageBinder(), Serialier), Serialier);

            bool disconnected = false;
            server.DisconnectClientSignal.Listen(l => disconnected = true);

            server.Start(new IPEndPoint(IPAddress.Loopback, 13892));

            var controllClient = new TcpClient();
            controllClient.Connect("localhost", 13892);
            controllClient.Close();

            Thread.Sleep(100);

            server.Stop();

            Assert.True(disconnected);
        }
    }
}
