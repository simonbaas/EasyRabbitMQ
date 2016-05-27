using System;
using System.Threading.Tasks;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    public interface ISubscriber : ITypedSubscriber, IDynamicSubscriber, IDisposable
    {
        void Start();
    }

    public interface ITypedSubscriber
    {
        IDisposable SubscribeQueue<T>(string queue, Func<Message<T>, Task> action);
        IDisposable SubscribeExchange<T>(string exchange, Func<Message<T>, Task> action);
        IDisposable SubscribeExchange<T>(string exchange, string queue, Func<Message<T>, Task> action);
        IDisposable SubscribeExchange<T>(string exchange, string queue, string routingKey, ExchangeType exchangeType, Func<Message<T>, Task> action);
        IDisposable SubscribeExchange<T>(string exchange, string routingKey, ExchangeType exchangeType, Func<Message<T>, Task> action);
    }

    public interface IDynamicSubscriber
    {
        IDisposable SubscribeQueue(string queue, Func<Message<dynamic>, Task> action);
        IDisposable SubscribeExchange(string exchange, Func<Message<dynamic>, Task> action);
        IDisposable SubscribeExchange(string exchange, string queue, Func<Message<dynamic>, Task> action);
        IDisposable SubscribeExchange(string exchange, string queue, string routingKey, ExchangeType exchangeType, Func<Message<dynamic>, Task> action);
        IDisposable SubscribeExchange(string exchange, string routingKey, ExchangeType exchangeType, Func<Message<dynamic>, Task> action);
    }
}
