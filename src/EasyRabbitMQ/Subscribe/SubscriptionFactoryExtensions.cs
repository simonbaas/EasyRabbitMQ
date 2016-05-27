using System;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    internal static class SubscriptionFactoryExtensions
    {
        public static ISubscription<T> SubscribeQueue<T>(this ISubscriptionFactory subscriptionFactory, string queue)
        {
            if (string.IsNullOrEmpty(queue)) throw new ArgumentNullException(nameof(queue));

            var connectionAndChannel = subscriptionFactory.ChannelFactory.CreateChannel();
            var serializer = subscriptionFactory.Serializer;
            var loggerFactory = subscriptionFactory.LoggerFactory;
            return new QueueAsyncSubscription<T>(connectionAndChannel, serializer, loggerFactory, queue);
        }

        public static ISubscription<T> SubscribeExchange<T>(this ISubscriptionFactory subscriptionFactory, string exchange,
            string queue, string routingKey, ExchangeType exchangeType)
        {
            if (string.IsNullOrEmpty(exchange)) throw new ArgumentNullException(nameof(exchange));
            if (string.IsNullOrEmpty(queue)) throw new ArgumentNullException(nameof(queue));
            if (routingKey == null) throw new ArgumentNullException(nameof(routingKey));

            var connectionAndChannel = subscriptionFactory.ChannelFactory.CreateChannel();
            var serializer = subscriptionFactory.Serializer;
            var loggerFactory = subscriptionFactory.LoggerFactory;
            return new ExchangeAndQueueAsyncSubscription<T>(connectionAndChannel, serializer, loggerFactory, 
                exchange, queue, routingKey, exchangeType);
        }

        public static ISubscription<T> SubscribeExchange<T>(this ISubscriptionFactory subscriptionFactory, string exchange,
            string routingKey, ExchangeType exchangeType)
        {
            if (string.IsNullOrEmpty(exchange)) throw new ArgumentNullException(nameof(exchange));
            if (routingKey == null) throw new ArgumentNullException(nameof(routingKey));

            var connectionAndChannel = subscriptionFactory.ChannelFactory.CreateChannel();
            var serializer = subscriptionFactory.Serializer;
            var loggerFactory = subscriptionFactory.LoggerFactory;
            return new ExchangeAsyncSubscription<T>(connectionAndChannel, serializer, loggerFactory, exchange, routingKey, exchangeType);
        }
    }
}
