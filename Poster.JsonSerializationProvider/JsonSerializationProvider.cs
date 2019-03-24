using System;
using System.IO;
using Newtonsoft.Json;

namespace Poster.JsonSerializationProvider
{
    public class JsonSerializationProvider : ISerializationProvider
    {
        private readonly JsonSerializer _jsonSerializer;

        public JsonSerializationProvider()
        {
            _jsonSerializer = new JsonSerializer();
        }
        
        public byte[] Serialize(object source)
        {
            byte[] result = null;
            using (var stream = new MemoryStream())
            using (var textWriter = new StreamWriter(stream))
            using (var writer = new JsonTextWriter(textWriter))
            {
                _jsonSerializer.Serialize(writer, source);
                textWriter.Flush();
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
            using (var textReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                result = _jsonSerializer.Deserialize(jsonReader, type);
            }
            return result;
        }
    }

}