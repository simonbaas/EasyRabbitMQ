using System;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Publish
{
    public interface IPublisher : IDisposable
    {
        void PublishQueue(string queue, dynamic message);
        void PublishQueue(string queue, MessageProperties messageProperties, dynamic message);
        void PublishExchange(string exchange, string routingKey, dynamic message);
        void PublishExchange(string exchange, string routingKey, MessageProperties messageProperties, dynamic message);
    }
}
