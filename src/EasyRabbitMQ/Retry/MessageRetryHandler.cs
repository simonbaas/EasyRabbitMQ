﻿using System;
using System.Collections.Generic;
using EasyRabbitMQ.Constants;
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

        internal MessageRetryHandler(Channel channel)
        {
            _channel = channel;

            _channel.EnableFairDispatch();
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
                _channel.Instance.BasicPublish("", Queues.Delayed, ea.BasicProperties, ea.Body);
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
            headers.AddOrUpdate(Headers.Exchange, ea.Exchange);
            headers.AddOrUpdate(Headers.RoutingKey, ea.RoutingKey);
            headers?.Remove(Headers.XDeath);
            headers?.Remove(Headers.Retries);

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

            Initialize();

            var channel = _channel.Instance;

            _consumer = new EventingBasicConsumer(channel);
            _consumer.Received += ConsumerOnReceived;

            channel.BasicConsume(
                queue: Queues.Retry,
                noAck: false,
                consumer: _consumer);
        }

        private void Initialize()
        {
            var channel = _channel.Instance;

            channel.ExchangeDeclare(
                exchange: Exchanges.Retry,
                type: ExchangeType.Direct,
                durable: true);

            channel.QueueDeclare(
                queue: Queues.Retry,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.QueueBind(
                queue: Queues.Retry,
                exchange: Exchanges.Retry,
                routingKey: RetryRoutingKey);

            channel.QueueDeclare(
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

        private void ConsumerOnReceived(object sender, BasicDeliverEventArgs ea)
        {
            try
            {
                var exchange = ea.BasicProperties.Headers.GetString(Headers.Exchange);
                var routingKey = ea.BasicProperties.Headers.GetString(Headers.RoutingKey);

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
