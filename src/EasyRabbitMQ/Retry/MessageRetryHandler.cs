using System;
using System.Collections.Generic;
using EasyRabbitMQ.Constants;
using EasyRabbitMQ.Extensions;
using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ExchangeType = RabbitMQ.Client.ExchangeType;
using static EasyRabbitMQ.Retry.MesageRetryConstants;
using Headers = EasyRabbitMQ.Constants.Headers;

namespace EasyRabbitMQ.Retry
{
    internal class MessageRetryHandler : IMessageRetryHandler
    {
        private readonly IChannelFactory _channelFactory;
        private int _maxNumberOfRetries;
        private string _failureQueue;
        private EventingBasicConsumer _consumer;
        private Lazy<Channel> _retryChannel;
        private readonly ILogger _logger = LogManager.GetLogger(typeof(MessageRetryHandler));

        internal MessageRetryHandler(IChannelFactory channelFactory)
        {
            _channelFactory = channelFactory;
            _retryChannel = new Lazy<Channel>(() => _channelFactory.CreateChannel());
        }

        public void SetMaxNumberOfRetries(int maxNumberOfRetries)
        {
            _maxNumberOfRetries = maxNumberOfRetries > 0 ? maxNumberOfRetries : 0;
        }

        public bool ShouldRetryMessage(BasicDeliverEventArgs ea)
        {
            if (_maxNumberOfRetries <= 0) return false;

            var retries = ea.BasicProperties.Headers.Get<int>(Headers.Retries);
            return retries < _maxNumberOfRetries;
        }

        public void RetryMessage(BasicDeliverEventArgs ea)
        {
            if (_maxNumberOfRetries <= 0) throw new InvalidOperationException("Number of retries must exceed 0 to enable message retry.");

            var headers = ea.BasicProperties.Headers;

            var retries = headers.Get<int>(Headers.Retries) + 1;
            headers.AddOrUpdate(Headers.Retries, retries);
            headers.AddOrUpdate(Headers.Exchange, ea.Exchange);
            headers.AddOrUpdate(Headers.RoutingKey, ea.RoutingKey);

            try
            {
                PublishToQueue(Queues.Delayed, ea.BasicProperties, ea.Body);
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

            using (var channel = _channelFactory.CreateChannel())
            {
                channel.Instance.QueueDeclare(
                    queue: failureQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
            }
        }

        public bool ShouldSendToFailureQueue()
        {
            return !string.IsNullOrWhiteSpace(_failureQueue);
        }

        public void SendToFailureQueue(BasicDeliverEventArgs ea)
        {
            if (string.IsNullOrWhiteSpace(_failureQueue)) return;

            var headers = ea.BasicProperties.Headers;
            headers.AddOrUpdate(Headers.Exchange, ea.Exchange);
            headers.AddOrUpdate(Headers.RoutingKey, ea.RoutingKey);
            headers?.Remove(Headers.XDeath);
            headers?.Remove(Headers.Retries);

            try
            {
                PublishToQueue(_failureQueue, ea.BasicProperties, ea.Body);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send message to failure queue '{_failureQueue}'. Message could potentially be lost.", ex);
            }
        }

        private void PublishToQueue(string queueName, IBasicProperties basicProperties, byte[] body)
        {
            using (var channel = _channelFactory.CreateChannel())
            {
                channel.Instance.BasicPublish("", queueName, basicProperties, body);
            }
        }

        public void Start()
        {
            if (_maxNumberOfRetries <= 0) return;

            Initialize();

            var channel = _retryChannel.Value;

            channel.EnableFairDispatch();

            _consumer = new EventingBasicConsumer(channel.Instance);
            _consumer.Received += ConsumerOnReceived;

            channel.Instance.BasicConsume(
                queue: Queues.Retry,
                noAck: false,
                consumer: _consumer);
        }

        private void Initialize()
        {
            using (var channel = _channelFactory.CreateChannel())
            {
                channel.Instance.ExchangeDeclare(
                    exchange: Exchanges.Retry,
                    type: ExchangeType.Direct,
                    durable: true);

                channel.Instance.QueueDeclare(
                    queue: Queues.Retry,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                channel.Instance.QueueBind(
                    queue: Queues.Retry,
                    exchange: Exchanges.Retry,
                    routingKey: RetryRoutingKey);

                channel.Instance.QueueDeclare(
                    queue: Queues.Delayed,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object>
                    {
                    {RabbitMQ.Client.Headers.XDeadLetterExchange, Exchanges.Retry},
                    {RabbitMQ.Client.Headers.XDeadLetterRoutingKey, RetryRoutingKey},
                    {RabbitMQ.Client.Headers.XMessageTTL, RetryDelay }
                    });
            }
        }

        private void ConsumerOnReceived(object sender, BasicDeliverEventArgs ea)
        {
            try
            {
                var exchange = ea.BasicProperties.Headers.GetString(Headers.Exchange);
                var routingKey = ea.BasicProperties.Headers.GetString(Headers.RoutingKey);

                _retryChannel.Value.Instance.BasicPublish(exchange, routingKey, ea.BasicProperties, ea.Body);

                _retryChannel.Value.Instance.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to requeue message for retry. Trying to send message to fail queue '{_failureQueue}'.", ex);

                _retryChannel.Value.Instance.BasicNack(ea.DeliveryTag, false, false);

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

                    if (_retryChannel.IsValueCreated)
                    {
                        _retryChannel.Value.Dispose();
                        _retryChannel = null;
                    }
                }

                _disposedValue = true;
            }
        }
    }
}
