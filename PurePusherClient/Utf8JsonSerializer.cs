using PurePusher.Interfaces;
using Utf8Json;

namespace PurePusher
{
    internal class Utf8JsonSerializer : ISerializer
    {
        public T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json);

        public byte[] Serialize(object obj) => JsonSerializer.Serialize(obj);
    }
}
