using System;
using Freud;

namespace Poster.Freud
{
    public class FreudSerializationProvider : ISerializationProvider
    {
        public FreudManager Freud { get; private set; }

        public FreudSerializationProvider()
        {
            Freud = new FreudManager();
        }

        public FreudSerializationProvider(FreudManager freud)
        {
            Freud = freud;
        }

        public byte[] Serialize(object source)
        {
            return Freud.Serialize(source.GetType(), source);
        }

        public T Deserialize<T>(byte[] data)
        {
            return Freud.Deserialize<T>(data);
        }

        public object Deserialize(Type type, byte[] data)
        {
            return Freud.Deserialize(type, data);
        }
    }
}
