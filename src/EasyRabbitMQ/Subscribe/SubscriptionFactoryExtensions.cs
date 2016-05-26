using System;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    internal static class SubscriptionFactoryExtensions
    {
        public static ISubscription SubscribeQueue(this ISubscriptionFactory subscriptionFactory, string queue)
        {
            if (string.IsNullOrEmpty(queue)) throw new ArgumentNullException(nameof(queue));

            var connectionAndChannel = subscriptionFactory.ChannelFactory.CreateChannel();
            var serializer = subscriptionFactory.Serializer;
            return new QueueSubscription(connectionAndChannel, serializer, queue);
        }

        public static ISubscription SubscribeExchange(this ISubscriptionFactory subscriptionFactory, string exchange,
            string queue, string routingKey, ExchangeType exchangeType)
        {
            if (string.IsNullOrEmpty(exchange)) throw new ArgumentNullException(nameof(exchange));
            if (string.IsNullOrEmpty(queue)) throw new ArgumentNullException(nameof(queue));
            if (string.IsNullOrEmpty(routingKey)) throw new ArgumentNullException(nameof(routingKey));

            var connectionAndChannel = subscriptionFactory.ChannelFactory.CreateChannel();
            var serializer = subscriptionFactory.Serializer;
            return new ExchangeAndQueueSubscription(connectionAndChannel, serializer, exchange, queue, routingKey, exchangeType);
        }

        public static ISubscription SubscribeExchange(this ISubscriptionFactory subscriptionFactory, string exchange,
            string routingKey, ExchangeType exchangeType)
        {
            if (string.IsNullOrEmpty(exchange)) throw new ArgumentNullException(nameof(exchange));
            if (string.IsNullOrEmpty(routingKey)) throw new ArgumentNullException(nameof(routingKey));

            var connectionAndChannel = subscriptionFactory.ChannelFactory.CreateChannel();
            var serializer = subscriptionFactory.Serializer;
            return new ExchangeSubscription(connectionAndChannel, serializer, exchange, routingKey, exchangeType);
        }
    }
}
