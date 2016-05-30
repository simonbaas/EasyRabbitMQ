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
        IDisposable SubscribeQueue<TMessage>(string queue, Func<Message<TMessage>, Task> action);
        IDisposable SubscribeQueue<TMessage, THandler>(string queue) where THandler : IHandleMessagesAsync<TMessage>;
        IDisposable SubscribeExchange<TMessage>(string exchange, Func<Message<TMessage>, Task> action, string queue = "", string routingKey = "", ExchangeType exchangeType = ExchangeType.Topic);
        IDisposable SubscribeExchange<TMessage, THandler>(string exchange, string queue = "", string routingKey = "", ExchangeType exchangeType = ExchangeType.Topic) where THandler : IHandleMessagesAsync<TMessage>;
    }

    public interface IDynamicSubscriber
    {
        IDisposable SubscribeQueue(string queue, Func<Message<dynamic>, Task> action); 
        IDisposable SubscribeExchange(string exchange, Func<Message<dynamic>, Task> action, string queue = "", string routingKey = "", ExchangeType exchangeType = ExchangeType.Topic);
    }
}
