using System;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Publish
{
    public interface IPublisher : IDisposable
    {
        void PublishQueue<T>(string queue, T message, MessageProperties messageProperties = null);
        void PublishExchange<T>(string exchange, T message, string routingKey = "", MessageProperties messageProperties = null);
    }
}
