using System;
using System.Threading.Tasks;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    public interface ISubscriber : IDisposable
    {
        IDisposable SubscribeQueue(string queue, Func<dynamic, Task> action);
        IDisposable SubscribeExchange(string exchange, Func<dynamic, Task> action);
        IDisposable SubscribeExchange(string exchange, string queue, Func<dynamic, Task> action);
        IDisposable SubscribeExchange(string exchange, string queue, string routingKey, ExchangeType exchangeType, Func<dynamic, Task> action);
        IDisposable SubscribeExchange(string exchange, string routingKey, ExchangeType exchangeType, Func<dynamic, Task> action);

        void Start();
    }
}
