using RabbitMQ.Client.Events;

namespace EasyRabbitMQ.Infrastructure
{
    public static class MessageFactory
    {
        public static Message Create(BasicDeliverEventArgs ea, dynamic payload)
        {
            var fields = CreateFields(ea);
            var properties = CreateProperties(ea);

            return new Message(fields, properties, payload);
        }

        private static Fields CreateFields(BasicDeliverEventArgs ea)
        {
            return new Fields
            {
                ConsumerTag = ea.ConsumerTag,
                DeliveryTag = ea.DeliveryTag,
                Exchange = ea.Exchange,
                Redelivered = ea.Redelivered,
                RoutingKey = ea.RoutingKey
            };
        }

        private static Properties CreateProperties(BasicDeliverEventArgs ea)
        {
            var basicProperties = ea.BasicProperties;
            return new Properties
            {
                AppId = basicProperties.AppId,
                ClusterId = basicProperties.ClusterId,
                ContentEncoding = basicProperties.ContentEncoding,
                ContentType = basicProperties.ContentType,
                CorrelationId = basicProperties.CorrelationId,
                DeliveryMode = basicProperties.DeliveryMode,
                Expiration = basicProperties.Expiration,
                Headers = basicProperties.Headers,
                MessageId = basicProperties.MessageId,
                Persistent = basicProperties.Persistent,
                Priority = basicProperties.Priority,
                ReplyTo = basicProperties.ReplyTo,
                Timestamp = basicProperties.Timestamp.UnixTime,
                Type = basicProperties.Type,
                UserId = basicProperties.UserId
            };
        }
    }
}