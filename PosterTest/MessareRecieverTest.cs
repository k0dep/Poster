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
        IMessageBindAccess createBindAccess(params MessageBindInstance[] binds)
        {            
            var mock = new Mock<IMessageBindAccess>();
            mock.Setup(t => t.AnyBind("message")).Returns(true);
            mock.Setup(t => t.GetBinds("message")).Returns(binds);
            mock.Setup(t => t.AnyBind("message1")).Returns(true);
            mock.Setup(t => t.GetBinds("message1")).Returns(binds);
            return mock.Object;
        }

        struct SerializePair
        {
            public object Instance;
            public byte[] Data;

            public SerializePair(object instance, byte[] data)
            {
                Instance = instance;
                Data = data;
            }
        }

        private ISerializationProvider createSerializer(params SerializePair[] data)
        {
            var mock = new Mock<ISerializationProvider>();

            foreach (var instance in data)
            {
                mock.Setup(t => t.Deserialize(instance.Instance.GetType(), instance.Data)).Returns(instance.Instance);
                mock.Setup(t => t.Serialize(instance.Instance)).Returns(instance.Data);
            }
            
            return mock.Object;
        }

        [Test]
        public void TestRecv()
        {
            var data = new byte[] { 0 };
            var isCallBind = false;
            var recv = new MessageReceiver(
                createBindAccess(new MessageBindInstance(o => isCallBind = true, typeof(string))),
                createSerializer(new SerializePair("message", data)));
            recv.Receive("message", data, null);
            Assert.True(isCallBind);
        }

        [Test]
        public void TestMultiBinds()
        {
            var data = new byte[] { 0 };
            var calls = 0;
            var recv = new MessageReceiver(
                createBindAccess(new MessageBindInstance(o => calls++, typeof(string)),
                    new MessageBindInstance(o => calls++, typeof(string))),
                createSerializer(new SerializePair("message", data)));
            recv.Receive("message", data, null);
            Assert.AreEqual(2, calls);
        }

        [Test]
        public void ShouldSaveHistory()
        {
            var data = new byte[] { 0 };
            var recv = new MessageReceiver(
                createBindAccess(new MessageBindInstance(o => { }, typeof(string))),
                createSerializer(new SerializePair("message", data)));
            recv.Receive("message", data, null);

            Assert.True(recv.MessageHistory.Any(t =>
                t.Message == data && t.Name == "message" && t.CustomData == null &&
                t.Timestamp < DateTime.Now));
        }

        [Test]
        public void ShouldRotateHistory()
        {
            var firstObj = new byte[] { 0 };
            var lastObj = new byte[] { 1 };
            var max = 100;

            var recv = new MessageReceiver(
                createBindAccess(new MessageBindInstance(o => { }, typeof(string))),
                createSerializer(new SerializePair("message", firstObj), new SerializePair("message1", lastObj)),
                max);

            recv.Receive("message", firstObj, null);

            for (int i = 0; i < max; i++)
            {
                recv.Receive("message1", lastObj, i);
            }

            Assert.True(recv.MessageHistory.All(t => t.Message != firstObj));

            Assert.True(recv.MessageHistory.All(t =>
                t.Message == lastObj && t.Name == "message1" && (int)t.CustomData < max));
        }
    }
}
