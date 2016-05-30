using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    internal interface ISubscriptionFactory
    {
        ISubscription<T> SubscribeQueue<T>(string queue);
        ISubscription<T> SubscribeExchange<T>(string exchange, string queue, string routingKey, ExchangeType exchangeType);
    }
}