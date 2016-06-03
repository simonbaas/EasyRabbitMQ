using System.Collections.Generic;

namespace EasyRabbitMQ.Infrastructure
{
    public class MessageProperties
    {
        public string CorrelationId { get; set; }
        public int? Expiration { get; set; }
        public IDictionary<string, object> Headers { get; set; }
        public string MessageId { get; set; }
        public bool PersistentMessage { get; set; } = true;
    }
}
