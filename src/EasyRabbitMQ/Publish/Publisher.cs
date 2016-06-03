using System;
using System.Collections.Generic;
using System.Globalization;
using EasyRabbitMQ.Configuration;
using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Serialization;
using RabbitMQ.Client;
using Headers = EasyRabbitMQ.Constants.Headers;

namespace EasyRabbitMQ.Publish
{
    internal class Publisher : IPublisher
    {
        private readonly ISerializer _serializer;
        private readonly Lazy<Channel> _channel;

        internal Publisher(EasyRabbitMQConfigurer configurer)
        {
            _serializer = configurer.Serializer;
            _channel = new Lazy<Channel>(() => configurer.ChannelFactory.CreateChannel());
        }

        public void PublishQueue<TMessage>(string queue, TMessage message, MessageProperties messageProperties = null)
        {
            if (string.IsNullOrWhiteSpace(queue)) throw new ArgumentNullException(nameof(queue));
            if (message == null) throw new ArgumentNullException(nameof(message));

            PublishMessageInternal("", queue, messageProperties, message);
        }

        public void PublishExchange<TMessage>(string exchange, TMessage message, string routingKey = "", MessageProperties messageProperties = null)
        {
            if (string.IsNullOrWhiteSpace(exchange)) throw new ArgumentNullException(nameof(exchange));
            if (routingKey == null) throw new ArgumentNullException(nameof(routingKey));
            if (message == null) throw new ArgumentNullException(nameof(message));

            PublishMessageInternal(exchange, routingKey, messageProperties, message);
        }

        private void PublishMessageInternal<TMessage>(string exchange, string routingKey, MessageProperties messageProperties, TMessage message)
        {
            var body = _serializer.Serialize(message);

            var channel = _channel.Value.Instance;

            lock (channel)
            {
                var props = CreateBasicProperties(channel, messageProperties);

                channel.BasicPublish(exchange, routingKey, props, body);
            }
        }

        private IBasicProperties CreateBasicProperties(IModel channel, MessageProperties messageProperties)
        {
            if (messageProperties == null) messageProperties = new MessageProperties();

            var props = channel.CreateBasicProperties();
            props.ContentType = _serializer.ContentType;

            if (messageProperties.CorrelationId != null)
            {
                props.CorrelationId = messageProperties.CorrelationId;
            }

            if (messageProperties.Expiration.HasValue)
            {
                props.Expiration = messageProperties.Expiration.Value.ToString(CultureInfo.InvariantCulture);
            }

            props.Headers = messageProperties.Headers;

            if (messageProperties.MessageId != null)
            {
                props.MessageId = messageProperties.MessageId;
            }

            props.Persistent = messageProperties.PersistentMessage;

            AddApplicationHeader(props);

            return props;
        }

        private static void AddApplicationHeader(IBasicProperties basicProperties)
        {
            if (basicProperties.Headers == null) basicProperties.Headers = new Dictionary<string, object>();
            basicProperties.Headers.Add(Headers.MessageId, Guid.NewGuid().ToString());
            basicProperties.Headers.Add(Headers.CorrelationId, Guid.NewGuid().ToString());
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
                    if (_channel.IsValueCreated)
                    {
                        _channel.Value?.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }
    }
}