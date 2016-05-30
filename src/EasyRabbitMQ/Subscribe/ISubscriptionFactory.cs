using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    internal interface ISubscriptionFactory
    {
        AbstractAsyncSubscription<TMessage> SubscribeQueue<TMessage>(string queue);
        AbstractAsyncSubscription<TMessage> SubscribeExchange<TMessage>(string exchange, string queue, string routingKey, ExchangeType exchangeType);
    }
}