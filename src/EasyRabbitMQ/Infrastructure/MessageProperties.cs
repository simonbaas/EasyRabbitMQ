using System;
using System.Collections.Generic;

namespace EasyRabbitMQ.Infrastructure
{
    public class MessageProperties
    {
        public bool PersistentMessage { get; set; } = true;
        public int? Expiration { get; set; }
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public IDictionary<string, object> Headers { get; set; } 
    }
}
