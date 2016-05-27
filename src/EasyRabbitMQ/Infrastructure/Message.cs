using System.Collections.Generic;

namespace EasyRabbitMQ.Infrastructure
{
    public class Message
    {
        public Fields Fields { get; }
        public Properties Properties { get; }
        public dynamic Payload { get; }

        internal Message(Fields fields, Properties properties, dynamic payload)
        {
            Fields = fields;
            Properties = properties;
            Payload = payload;
        }
    }

    public class Fields
    {
        public string ConsumerTag { get; set; }
        public ulong DeliveryTag { get; set; }
        public string Exchange { get; set; }
        public bool Redelivered { get; set; }
        public string RoutingKey { get; set; }
    }

    public class Properties
    {
        public string AppId { get; set; }
        public string ClusterId { get; set; }
        public string ContentEncoding { get; set; }
        public string ContentType { get; set; }
        public string CorrelationId { get; set; }
        public byte DeliveryMode { get; set; }
        public string Expiration { get; set; }
        public IDictionary<string, object> Headers { get; set; }
        public string MessageId { get; set; }
        public bool Persistent { get; set; }
        public byte Priority { get; set; }
        public string ReplyTo { get; set; }
        public long? Timestamp { get; set; }
        public string Type { get; set; }
        public string UserId { get; set; }
    }
}
