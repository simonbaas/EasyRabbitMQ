using System;
using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Retry;
using EasyRabbitMQ.Serialization;

namespace EasyRabbitMQ.Subscribe
{
    internal class SubscriptionFactory : ISubscriptionFactory
    {
        private readonly IChannelFactory _channelFactory;
        private readonly ISerializer _serializer;
        private readonly IHandlerActivator _handlerActivator;
        private readonly IMessageRetryHandlerFactory _messageRetryHandlerFactory;

        public SubscriptionFactory(IChannelFactory channelFactory, ISerializer serializer, IHandlerActivator handlerActivator, 
            IMessageRetryHandlerFactory messageRetryHandlerFactory)
        {
            _channelFactory = channelFactory;
            _serializer = serializer;
            _handlerActivator = handlerActivator;
            _messageRetryHandlerFactory = messageRetryHandlerFactory;
        }

        public AbstractAsyncSubscription<TMessage> SubscribeQueue<TMessage>(string queue) where TMessage : class 
        {
            if (string.IsNullOrEmpty(queue)) throw new ArgumentNullException(nameof(queue));

            var channel = _channelFactory.CreateChannel();
            var messageDispatcher = new MessageDispatcher<TMessage>(_handlerActivator);
            var messageRetryHandler = _messageRetryHandlerFactory.CreateHandler();
            return new QueueAsyncSubscription<TMessage>(channel, _serializer, messageDispatcher, messageRetryHandler, queue);
        }

        public AbstractAsyncSubscription<TMessage> SubscribeExchange<TMessage>(string exchange, string queue, string routingKey, ExchangeType exchangeType)
            where TMessage : class 
        {
            if (string.IsNullOrEmpty(exchange)) throw new ArgumentNullException(nameof(exchange));
            if (queue == null) throw new ArgumentNullException(nameof(queue));
            if (routingKey == null) throw new ArgumentNullException(nameof(routingKey));

            var channel = _channelFactory.CreateChannel();
            var messageDispatcher = new MessageDispatcher<TMessage>(_handlerActivator);
            var messageRetryHandler = _messageRetryHandlerFactory.CreateHandler();
            return new ExchangeAsyncSubscription<TMessage>(channel, _serializer, messageDispatcher, messageRetryHandler,
                exchange, queue, routingKey, exchangeType);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _channelFactory.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}