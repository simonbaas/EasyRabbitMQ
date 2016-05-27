using System;
using System.Text;
using Newtonsoft.Json;

namespace EasyRabbitMQ.Serialization
{
    internal class DefaultJsonSerializer : ISerializer
    {
        private readonly Encoding _textEncoding = Encoding.UTF8;

        public string ContentType => "application/json";

        public T Deserialize<T>(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            var str = _textEncoding.GetString(bytes);

            return JsonConvert.DeserializeObject<T>(str);
        }

        public byte[] Serialize<T>(T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var str = JsonConvert.SerializeObject(value, Formatting.None);

            return _textEncoding.GetBytes(str);
        }
    }
}