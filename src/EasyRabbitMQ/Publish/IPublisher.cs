using System;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Publish
{
    public interface IPublisher : IDisposable
    {
        void PublishQueue<TMessage>(string queue, TMessage message, MessageProperties messageProperties = null);
        void PublishExchange<TMessage>(string exchange, TMessage message, string routingKey = "", MessageProperties messageProperties = null);
    }
}
