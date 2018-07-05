using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Poster.DefaultSerializationProvider
{
    public class BinaryFormatterSerializationProvider : ISerializationProvider
    {
        private readonly BinaryFormatter serializer;

        public BinaryFormatterSerializationProvider()
        {
            serializer = new BinaryFormatter();
        }

        public byte[] Serialize(object source)
        {
            byte[] result = null;
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, source);
                result = stream.ToArray();
            }

            return result;
        }

        public T Deserialize<T>(byte[] data)
        {
            return (T)Deserialize(typeof(T), data);
        }

        public object Deserialize(Type type, byte[] data)
        {
            object result = null;
            using (var stream = new MemoryStream(data))
            {
                result = serializer.Deserialize(stream);
            }
            return result;
        }
    }
}
