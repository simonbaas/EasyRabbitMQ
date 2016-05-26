using System;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    public interface ISubscriber : IDisposable
    {
        IDisposable SubscribeQueue(string queue, Action<dynamic> action);
        IDisposable SubscribeExchange(string exchange, Action<dynamic> action);
        IDisposable SubscribeExchange(string exchange, string queue, Action<dynamic> action);
        IDisposable SubscribeExchange(string exchange, string queue, string routingKey, ExchangeType exchangeType, Action<dynamic> action);
        IDisposable SubscribeExchange(string exchange, string routingKey, ExchangeType exchangeType, Action<dynamic> action);

        void Start();
    }
}
