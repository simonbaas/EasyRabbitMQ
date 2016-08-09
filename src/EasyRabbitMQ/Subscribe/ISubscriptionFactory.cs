using System;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    internal interface ISubscriptionFactory : IDisposable
    {
        AbstractAsyncSubscription<TMessage> SubscribeQueue<TMessage>(string queue) where TMessage : class;
        AbstractAsyncSubscription<TMessage> SubscribeExchange<TMessage>(string exchange, string queue, string routingKey, ExchangeType exchangeType) where TMessage: class;
    }
}