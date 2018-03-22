using System;

namespace Poster
{

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MessageAttribute : Attribute
    {
        public string MessageName;

        public MessageAttribute(string messageName)
        {
            MessageName = messageName;
        }
    }
}
