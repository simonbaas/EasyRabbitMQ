using System;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    public interface ISubscriber : IDisposable
    {
        IDisposable On(string queue, Action<dynamic> action);
        IDisposable On(string exchange, string queue, string routingKey, ExchangeType exchangeType, Action<dynamic> action);
        IDisposable On(string exchange, string routingKey, ExchangeType exchangeType, Action<dynamic> action);

        void Start();
    }
}
