using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    internal interface ISubscriptionFactory
    {
        AbstractAsyncSubscription<T> SubscribeQueue<T>(string queue);
        AbstractAsyncSubscription<T> SubscribeExchange<T>(string exchange, string queue, string routingKey, ExchangeType exchangeType);
    }
}