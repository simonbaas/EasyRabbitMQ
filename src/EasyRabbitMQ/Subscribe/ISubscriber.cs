using System;
using System.Threading.Tasks;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    public interface ISubscriber : IDisposable
    {
        IDisposable SubscribeQueue(string queue, Func<Message, Task> action);
        IDisposable SubscribeExchange(string exchange, Func<Message, Task> action);
        IDisposable SubscribeExchange(string exchange, string queue, Func<Message, Task> action);
        IDisposable SubscribeExchange(string exchange, string queue, string routingKey, ExchangeType exchangeType, Func<Message, Task> action);
        IDisposable SubscribeExchange(string exchange, string routingKey, ExchangeType exchangeType, Func<Message, Task> action);

        void Start();
    }
}
