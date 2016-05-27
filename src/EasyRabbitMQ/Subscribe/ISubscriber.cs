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
        IDisposable SubscribeExchange<T>(string exchange, Func<Message<T>, Task> action, string queue = "", string routingKey = "", ExchangeType exchangeType = ExchangeType.Topic);
    }

    public interface IDynamicSubscriber
    {
        IDisposable SubscribeQueue(string queue, Func<Message<dynamic>, Task> action);
        IDisposable SubscribeExchange(string exchange, Func<Message<dynamic>, Task> action, string queue = "", string routingKey = "", ExchangeType exchangeType = ExchangeType.Topic);
    }
}
