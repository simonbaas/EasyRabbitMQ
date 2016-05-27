using System;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Publish
{
    public interface IPublisher : IDisposable
    {
        void PublishQueue<T>(string queue, T message);
        void PublishQueue<T>(string queue, MessageProperties messageProperties, T message);
        void PublishExchange<T>(string exchange, T message);
        void PublishExchange<T>(string exchange, MessageProperties messageProperties, T message);
        void PublishExchange<T>(string exchange, string routingKey, T message);
        void PublishExchange<T>(string exchange, string routingKey, MessageProperties messageProperties, T message);
    }
}
