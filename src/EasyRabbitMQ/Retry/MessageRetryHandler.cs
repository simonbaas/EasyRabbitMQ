using System;
using System.Collections.Generic;
using EasyRabbitMQ.Extensions;
using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Logging;
using RabbitMQ.Client.Events;
using ExchangeType = RabbitMQ.Client.ExchangeType;
using static EasyRabbitMQ.Retry.MesageRetryConstants;

namespace EasyRabbitMQ.Retry
{
    internal class MessageRetryHandler : IMessageRetryHandler
    {
        private readonly int _maxNumberOfRetries;
        private readonly Channel _channel;
        private readonly ILogger _logger;

        public MessageRetryHandler(IChannelFactory channelFactory, ILoggerFactory loggerFactory, int maxNumberOfRetries)
        {
            if (maxNumberOfRetries <= 0) return;

            _maxNumberOfRetries = maxNumberOfRetries;
            _channel = channelFactory.CreateChannel();
            _logger = loggerFactory.GetLogger<MessageRetryHandler>();

            Initialize();
        }

        public virtual bool ShouldRetryMessage(BasicDeliverEventArgs ea)
        {
            if (_maxNumberOfRetries <= 0) return false;

            var retries = ea.BasicProperties.Headers.Get<int>(RetriesHeaderKey);
            return retries < _maxNumberOfRetries;
        }

        public virtual void RetryMessage(BasicDeliverEventArgs ea)
        {
            if (_maxNumberOfRetries <= 0) throw new InvalidOperationException("Number of retries must exceed 0 to enable message retry.");

            var headers = ea.BasicProperties.Headers;

            var retries = headers.Get<int>(RetriesHeaderKey) + 1;
            headers.AddOrUpdate(RetriesHeaderKey, retries);
            headers.AddOrUpdate(ExchangeHeaderKey, ea.Exchange);
            headers.AddOrUpdate(RoutingKeyHeaderKey, ea.RoutingKey);

            try
            {
                _channel.Instance.BasicPublish("", DelayedQueue, ea.BasicProperties, ea.Body);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to publish message to queue for delayed retry. Message is potentially lost.", ex);
            }
        }

        private void Initialize()
        {
            var channel = _channel.Instance;

            channel.ExchangeDeclare(
                exchange: RetryExchange,
                type: ExchangeType.Direct,
                durable: true);

            channel.QueueDeclare(
                queue: RetryQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.QueueBind(
                queue: RetryQueue,
                exchange: RetryExchange,
                routingKey: RetryRoutingKey);

            channel.QueueDeclare(
                queue: DelayedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    {DeadLetterExchangeHeaderKey, RetryExchange},
                    {DeadLetterRoutingKeyHeaderKey, RetryRoutingKey},
                    {MessageTtlHeaderKey, RetryDelay }
                });

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += ConsumerOnReceived;

            channel.BasicConsume(
                queue: RetryQueue,
                noAck: false,
                consumer: consumer);
        }

        private void ConsumerOnReceived(object sender, BasicDeliverEventArgs ea)
        {
            try
            {
                var exchange = ea.BasicProperties.Headers.GetString(ExchangeHeaderKey);
                var routingKey = ea.BasicProperties.Headers.GetString(RoutingKeyHeaderKey);

                _channel.Instance.BasicPublish(exchange, routingKey, ea.BasicProperties, ea.Body);

                _channel.Instance.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to requeue message for retry. Message is potentially lost.", ex);

                _channel.Instance.BasicNack(ea.DeliveryTag, false, false);
            }
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
                    _channel?.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
