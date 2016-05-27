﻿using System;
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
            var loggerFactory = subscriptionFactory.LoggerFactory;
            return new QueueAsyncSubscription(connectionAndChannel, serializer, loggerFactory, queue);
        }

        public static ISubscription SubscribeExchange(this ISubscriptionFactory subscriptionFactory, string exchange,
            string queue, string routingKey, ExchangeType exchangeType)
        {
            if (string.IsNullOrEmpty(exchange)) throw new ArgumentNullException(nameof(exchange));
            if (string.IsNullOrEmpty(queue)) throw new ArgumentNullException(nameof(queue));
            if (routingKey == null) throw new ArgumentNullException(nameof(routingKey));

            var connectionAndChannel = subscriptionFactory.ChannelFactory.CreateChannel();
            var serializer = subscriptionFactory.Serializer;
            var loggerFactory = subscriptionFactory.LoggerFactory;
            return new ExchangeAndQueueAsyncSubscription(connectionAndChannel, serializer, loggerFactory, 
                exchange, queue, routingKey, exchangeType);
        }

        public static ISubscription SubscribeExchange(this ISubscriptionFactory subscriptionFactory, string exchange,
            string routingKey, ExchangeType exchangeType)
        {
            if (string.IsNullOrEmpty(exchange)) throw new ArgumentNullException(nameof(exchange));
            if (routingKey == null) throw new ArgumentNullException(nameof(routingKey));

            var connectionAndChannel = subscriptionFactory.ChannelFactory.CreateChannel();
            var serializer = subscriptionFactory.Serializer;
            var loggerFactory = subscriptionFactory.LoggerFactory;
            return new ExchangeAsyncSubscription(connectionAndChannel, serializer, loggerFactory, exchange, routingKey, exchangeType);
        }
    }
}
