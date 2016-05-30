using System;
using System.Runtime.Remoting.Messaging;
using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Logging;
using EasyRabbitMQ.Retry;
using EasyRabbitMQ.Serialization;

namespace EasyRabbitMQ.Subscribe
{
    internal class SubscriptionFactory : ISubscriptionFactory
    {
        private readonly IChannelFactory _channelFactory;
        private readonly ISerializer _serializer;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMessageRetryHandler _messageRetryHandler;

        internal SubscriptionFactory(IChannelFactory channelFactory, ISerializer serializer, ILoggerFactory loggerFactory,
            IMessageRetryHandler messageRetryHandler)
        {
            _channelFactory = channelFactory;
            _serializer = serializer;
            _loggerFactory = loggerFactory;
            _messageRetryHandler = messageRetryHandler;
        }

        public ISubscription<T> SubscribeQueue<T>(string queue)
        {
            if (string.IsNullOrEmpty(queue)) throw new ArgumentNullException(nameof(queue));

            var channel = _channelFactory.CreateChannel();
            var serializer = _serializer;
            var loggerFactory = _loggerFactory;
            return new QueueAsyncSubscription<T>(channel, serializer, loggerFactory, _messageRetryHandler, queue);
        }

        public ISubscription<T> SubscribeExchange<T>(string exchange, string queue, string routingKey, ExchangeType exchangeType)
        {
            if (string.IsNullOrEmpty(exchange)) throw new ArgumentNullException(nameof(exchange));
            if (queue == null) throw new ArgumentNullException(nameof(queue));
            if (routingKey == null) throw new ArgumentNullException(nameof(routingKey));

            var channel = _channelFactory.CreateChannel();
            var serializer = _serializer;
            var loggerFactory = _loggerFactory;
            return new ExchangeAsyncSubscription<T>(channel, serializer, loggerFactory, _messageRetryHandler,
                exchange, queue, routingKey, exchangeType);
        }
    }
}