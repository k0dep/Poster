using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Poster;

namespace PosterTest
{
    [TestFixture]
    public class MessareRecieverTest
    {
        IMessageBindAccess createBindAccess(params MessageBindDelegate<object>[] binds)
        {            
            var mock = new Mock<IMessageBindAccess>();
            mock.Setup(t => t.AnyBind("message")).Returns(true);
            mock.Setup(t => t.GetBinds("message")).Returns(binds);
            return mock.Object;
        }

        [Test]
        public void TestRecv()
        {
            var isCallBind = false;
            var recv = new MessageReceiver(createBindAccess(o => isCallBind = true));
            recv.Receive("message", "data", null);
            Assert.True(isCallBind);
        }

        [Test]
        public void TestValidObject()
        {
            var data = "";
            var recv = new MessageReceiver(createBindAccess(o => data = (string)o));
            recv.Receive("message", "data", null);
            Assert.AreEqual("data", data);
        }

        [Test]
        public void TestMultiBinds()
        {
            var calls = 0;
            var recv = new MessageReceiver(createBindAccess(o => calls++, o => calls++));
            recv.Receive("message", null, null);
            Assert.AreEqual(2, calls);
        }

        [Test]
        public void ShouldSaveHistory()
        {        
            var recv = new MessageReceiver(createBindAccess(o => { }));
            recv.Receive("message", "message", null);

            Assert.True(recv.MessageHistory.Any(t =>
                (string) t.Message == "message" && t.Name == "message" && t.CustomData == null &&
                t.Timestamp < DateTime.Now));
        }

        [Test]
        public void ShouldRotateHistory()
        {
            var firstObj = "firstMessage";
            var lastObj = "lastObject";
            var max = 100;

            var recv = new MessageReceiver(createBindAccess(o => { }), max);

            recv.Receive("message", firstObj, null);

            for (int i = 0; i < max; i++)
            {
                recv.Receive("message", lastObj, i);
            }

            Assert.True(recv.MessageHistory.All(t => (string) t.Message != firstObj));

            Assert.True(recv.MessageHistory.All(t =>
                (string)t.Message == lastObj && t.Name == "message" && (int)t.CustomData < max));
        }
    }
}
