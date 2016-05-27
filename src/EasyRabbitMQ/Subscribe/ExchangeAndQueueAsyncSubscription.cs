﻿using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Logging;
using EasyRabbitMQ.Serialization;
using RabbitMQ.Client;
using ExchangeType = EasyRabbitMQ.Infrastructure.ExchangeType;

namespace EasyRabbitMQ.Subscribe
{
    internal class ExchangeAndQueueAsyncSubscription<T> : AbstractAsyncSubscription<T>
    {
        private readonly string _queue;
        private IModel _channel;

        public ExchangeAndQueueAsyncSubscription(Channel channel, ISerializer serializer, ILoggerFactory loggerFactory,
            string exchange, string queue, string routingKey, ExchangeType exchangeType) 
            : base(channel, serializer, loggerFactory)
        {
            _queue = queue;

            Initialize(exchange, routingKey, exchangeType);
        }

        private void Initialize(string exchange, string routingKey, ExchangeType exchangeType)
        {
            var channel = Channel.Instance;

            channel.ExchangeDeclare(
                exchange: exchange,
                type: exchangeType.GetRabbitMQExchangeType(),
                durable: true,
                autoDelete: false,
                arguments: null);

            channel.QueueDeclare(
                queue: _queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.QueueBind(
                queue: _queue,
                exchange: exchange,
                routingKey: routingKey);

            _channel = channel;
        }

        protected override IModel GetChannel()
        {
            return _channel;
        }

        protected override string GetQueue()
        {
            return _queue;
        }
    }
}
