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
        private int _maxNumberOfRetries;
        private string _failureQueue;
        private readonly Channel _channel;
        private EventingBasicConsumer _consumer;
        private readonly ILogger _logger = LogManager.GetLogger(typeof (MessageRetryHandler));

        public MessageRetryHandler(IChannelFactory channelFactory)
        {
            _channel = channelFactory.CreateChannel();

            _channel.EnableFairDispatch();

            Initialize();
        }

        public void SetMaxNumberOfRetries(int maxNumberOfRetries)
        {
            _maxNumberOfRetries = maxNumberOfRetries > 0 ? maxNumberOfRetries : 0;
        }

        public bool ShouldRetryMessage(BasicDeliverEventArgs ea)
        {
            if (_maxNumberOfRetries <= 0) return false;

            var retries = ea.BasicProperties.Headers.Get<int>(RetriesHeaderKey);
            return retries < _maxNumberOfRetries;
        }

        public void RetryMessage(BasicDeliverEventArgs ea)
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
                _logger.Error($"Failed to publish message to queue for delayed retry. Trying to send message to failure queue '{_failureQueue}'.", ex);

                SendToFailureQueue(ea);
            }
        }

        public void SetFailureQueue(string failureQueue)
        {
            if (string.IsNullOrWhiteSpace(failureQueue)) throw new ArgumentNullException(nameof(failureQueue));

            _failureQueue = failureQueue;

            _channel.Instance.QueueDeclare(
                queue: failureQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        public bool ShouldSendToFailureQueue()
        {
            return !string.IsNullOrWhiteSpace(_failureQueue);
        }

        public void SendToFailureQueue(BasicDeliverEventArgs ea)
        {
            if (string.IsNullOrWhiteSpace(_failureQueue)) return;

            var headers = ea.BasicProperties.Headers;
            headers.AddOrUpdate(ExchangeHeaderKey, ea.Exchange);
            headers.AddOrUpdate(RoutingKeyHeaderKey, ea.RoutingKey);
            headers?.Remove(XDeathHeaderKey);

            try
            {
                _channel.Instance.BasicPublish("", _failureQueue, ea.BasicProperties, ea.Body);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send message to failure queue '{_failureQueue}'. Message could potentially be lost.", ex);
            }
        }

        public void Start()
        {
            if (_maxNumberOfRetries <= 0) return;

            var channel = _channel.Instance;

            _consumer = new EventingBasicConsumer(channel);
            _consumer.Received += ConsumerOnReceived;

            channel.BasicConsume(
                queue: RetryQueue,
                noAck: false,
                consumer: _consumer);
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
                _logger.Error($"Failed to requeue message for retry. Trying to send message to fail queue '{_failureQueue}'.", ex);

                _channel.Instance.BasicNack(ea.DeliveryTag, false, false);

                SendToFailureQueue(ea);
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
                    if (_consumer != null)
                    {
                        _consumer.Received -= ConsumerOnReceived;
                        _consumer = null;
                    }

                    _channel?.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
