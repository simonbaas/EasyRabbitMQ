using System;
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
        private readonly IHandlerActivator _handlerActivator;
        private readonly IMessageRetryHandler _messageRetryHandler;

        internal SubscriptionFactory(IChannelFactory channelFactory, ISerializer serializer, IHandlerActivator handlerActivator, 
            IMessageRetryHandler messageRetryHandler)
        {
            _channelFactory = channelFactory;
            _serializer = serializer;
            _handlerActivator = handlerActivator;
            _messageRetryHandler = messageRetryHandler;
        }

        public AbstractAsyncSubscription<TMessage> SubscribeQueue<TMessage>(string queue)
        {
            if (string.IsNullOrEmpty(queue)) throw new ArgumentNullException(nameof(queue));

            var channel = _channelFactory.CreateChannel();
            var messageDispatcher = new MessageDispatcher<TMessage>(_handlerActivator);
            return new QueueAsyncSubscription<TMessage>(channel, _serializer, messageDispatcher, _messageRetryHandler, queue);
        }

        public AbstractAsyncSubscription<TMessage> SubscribeExchange<TMessage>(string exchange, string queue, string routingKey, ExchangeType exchangeType)
        {
            if (string.IsNullOrEmpty(exchange)) throw new ArgumentNullException(nameof(exchange));
            if (queue == null) throw new ArgumentNullException(nameof(queue));
            if (routingKey == null) throw new ArgumentNullException(nameof(routingKey));

            var channel = _channelFactory.CreateChannel();
            var messageDispatcher = new MessageDispatcher<TMessage>(_handlerActivator);
            return new ExchangeAsyncSubscription<TMessage>(channel, _serializer, messageDispatcher, _messageRetryHandler,
                exchange, queue, routingKey, exchangeType);
        }
    }
}