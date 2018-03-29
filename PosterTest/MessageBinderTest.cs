using System.Linq;
using NUnit.Framework;
using Poster;

namespace PosterTest
{
    [TestFixture(TestOf = typeof(MessageBinder))]
    public class MessageBinderTest
    {
        [Test]
        public void NamedBind()
        {
            var isCall = false;
            MessageBindDelegate<string> bindAction = s => isCall = true;;

            var binder = new MessageBinder();

            Is.False.ApplyTo(binder.AnyBind("message"));

            binder.Bind<string>("message", bindAction);

            Is.True.ApplyTo(binder.AnyBind("message"));
            Is.Not.Null.ApplyTo(binder.GetBinds("message"));
           
            Assert.AreEqual(1, binder.GetBinds("message").Count);

            binder.GetBinds("message")[0].BindAction(null);

            Assert.True(isCall);
        }

        [Test]
        public void ClassNamedBind()
        {
            var isCall = false;
            MessageBindDelegate<string> bindAction = s => isCall = true; ;

            var binder = new MessageBinder();

            Is.False.ApplyTo(binder.AnyBind(typeof(string).FullName));

            binder.Bind<string>(bindAction);

            Is.True.ApplyTo(binder.AnyBind(typeof(string).FullName));
            Is.Not.Null.ApplyTo(binder.GetBinds(typeof(string).FullName));

            Assert.AreEqual(1, binder.GetBinds(typeof(string).FullName).Count);

            binder.GetBinds(typeof(string).FullName)[0].BindAction(null);

            Assert.True(isCall);
        }

        [Test]
        public void ClassNamedAttributeBind()
        {
            var isCall = false;
            MessageBindDelegate<Foo> bindAction = s => isCall = true; ;

            var binder = new MessageBinder();

            Is.False.ApplyTo(binder.AnyBind("foo"));

            binder.Bind(bindAction);

            Is.True.ApplyTo(binder.AnyBind("foo"));
            Is.Not.Null.ApplyTo(binder.GetBinds("foo"));

            Assert.AreEqual(1, binder.GetBinds("foo").Count);

            binder.GetBinds("foo")[0].BindAction(null);

            Assert.True(isCall);
        }

        [Test]
        public void MulipleHandlerBind()
        {
            var res = 0;
            MessageBindDelegate<string> bindAction = s => { res |= 1; };
            MessageBindDelegate<string> bindAction2 = s => { res |= 2; };
            var binder = new MessageBinder();

            Is.False.ApplyTo(binder.AnyBind("message"));

            binder.Bind<string>("message", bindAction);
            binder.Bind<string>("message", bindAction2);

            Is.True.ApplyTo(binder.AnyBind("message"));
            Is.Not.Null.ApplyTo(binder.GetBinds("message"));
           
            Assert.AreEqual(2, binder.GetBinds("message").Count);

            foreach (var messageBindDelegate in binder.GetBinds("message"))
            {
                messageBindDelegate.BindAction(null);
            }

            Assert.AreEqual(3, res);
        }

        [Test]
        public void MulipleMessageBind()
        {
            var res = 0;
            MessageBindDelegate<string> bindAction = s => { res |= 1; };
            MessageBindDelegate<string> bindAction2 = s => { res |= 2; };

            var binder = new MessageBinder();

            Is.False.ApplyTo(binder.AnyBind("message"));

            binder.Bind<string>("message", bindAction);
            binder.Bind<string>("message1", bindAction2);

            Is.True.ApplyTo(binder.AnyBind("message"));
            Is.Not.Null.ApplyTo(binder.GetBinds("message"));

            Is.True.ApplyTo(binder.AnyBind("message1"));
            Is.Not.Null.ApplyTo(binder.GetBinds("message1"));

            Assert.AreEqual(1, binder.GetBinds("message").Count);
            Assert.AreEqual(1, binder.GetBinds("message1").Count);

            binder.GetBinds("message").First().BindAction(null);
            binder.GetBinds("message1").First().BindAction(null);

            Assert.AreEqual(3, res);
        }
    }

    [Message("foo")]
    public class Foo
    {
    }
}