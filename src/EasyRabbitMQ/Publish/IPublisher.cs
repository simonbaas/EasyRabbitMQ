using System;
using System.Threading.Tasks;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Publish
{
    public interface IPublisher : IDisposable
    {
        void PublishQueue<TMessage>(string queue, TMessage message, MessageProperties messageProperties = null);
        Task PublishQueueAsync<TMessage>(string queue, TMessage message, MessageProperties messageProperties = null);
        void PublishExchange<TMessage>(string exchange, TMessage message, string routingKey = "", MessageProperties messageProperties = null);
        Task PublishExchangeAsync<TMessage>(string exchange, TMessage message, string routingKey = "", MessageProperties messageProperties = null);
    }
}
