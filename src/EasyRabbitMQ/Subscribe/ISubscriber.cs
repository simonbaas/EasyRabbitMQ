using System;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    public interface ISubscriber : ITypedSubscriber, IDynamicSubscriber, IDisposable
    {
        void Start();
    }

    public interface ITypedSubscriber
    {
        ISubscription<TMessage> Queue<TMessage>(string queue) where TMessage : class;
        ISubscription<TMessage> Exchange<TMessage>(string exchange, string queue = "", string routingKey = "",
            ExchangeType exchangeType = ExchangeType.Topic) where TMessage : class;
    }

    public interface IDynamicSubscriber
    {
        ISubscription<dynamic> Queue(string queue);
        ISubscription<dynamic> Exchange(string exchange, string queue = "", string routingKey = "",
            ExchangeType exchangeType = ExchangeType.Topic);
    }
}
