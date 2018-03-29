using System;

namespace Poster
{
    public interface ISerializationProvider
    {
        byte[] Serialize(object source);
        T Deserialize<T>(byte[] data);
        object Deserialize(Type type, byte[] data);
    }
}
