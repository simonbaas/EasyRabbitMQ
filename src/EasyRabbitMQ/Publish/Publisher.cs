using System;
using System.Collections.Generic;
using System.Globalization;
using EasyRabbitMQ.Configuration;
using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Serialization;
using RabbitMQ.Client;

namespace EasyRabbitMQ.Publish
{
    internal class Publisher : IPublisher
    {
        private readonly ISerializer _serializer;
        private readonly Lazy<Channel> _channel;

        private static readonly KeyValuePair<string, object> CustomHeader = 
            new KeyValuePair<string, object>("x-sending-application", "EasyRabbitMQ");

        internal Publisher(EasyRabbitMQConfigurer configurer)
        {
            _serializer = configurer.Serializer;
            _channel = new Lazy<Channel>(() => configurer.ChannelFactory.CreateChannel());
        }

        public void PublishQueue(string queue, dynamic message)
        {
            if (string.IsNullOrWhiteSpace(queue)) throw new ArgumentNullException(nameof(queue));
            if (message == null) throw new ArgumentNullException(nameof(message));

            PublishMessageInternal("", queue, null, message);
        }

        public void PublishQueue(string queue, MessageProperties messageProperties, dynamic message)
        {
            if (string.IsNullOrWhiteSpace(queue)) throw new ArgumentNullException(nameof(queue));
            if (messageProperties == null) throw new ArgumentNullException(nameof(messageProperties));
            if (message == null) throw new ArgumentNullException(nameof(message));

            PublishMessageInternal("", queue, messageProperties, message);
        }

        public void PublishExchange(string exchange, string routingKey, dynamic message)
        {
            if (string.IsNullOrWhiteSpace(exchange)) throw new ArgumentNullException(nameof(exchange));
            if (string.IsNullOrWhiteSpace(routingKey)) throw new ArgumentNullException(nameof(routingKey));
            if (message == null) throw new ArgumentNullException(nameof(message));

            PublishMessageInternal(exchange, routingKey, null, message);
        }

        public void PublishExchange(string exchange, string routingKey, MessageProperties messageProperties,
            dynamic message)
        {
            if (string.IsNullOrWhiteSpace(exchange)) throw new ArgumentNullException(nameof(exchange));
            if (string.IsNullOrWhiteSpace(routingKey)) throw new ArgumentNullException(nameof(routingKey));
            if (messageProperties == null) throw new ArgumentNullException(nameof(messageProperties));
            if (message == null) throw new ArgumentNullException(nameof(message));

            PublishMessageInternal(exchange, routingKey, messageProperties, message);
        }

        private void PublishMessageInternal(string queueOrExchange, string routingKey,
            MessageProperties messageProperties, dynamic message)
        {
            var channel = _channel.Value.Instance;

            var body = _serializer.Serialize(message);

            var props = CreateBasicProperties(channel, messageProperties);

            channel.BasicPublish(queueOrExchange, routingKey, props, body);
        }

        private IBasicProperties CreateBasicProperties(IModel channel, MessageProperties messageProperties)
        {
            if (messageProperties == null) messageProperties = new MessageProperties();

            var props = channel.CreateBasicProperties();
            props.ContentType = _serializer.ContentType;
            props.Persistent = messageProperties.PersistentMessage;
            props.MessageId = messageProperties.MessageId;
            
            if (messageProperties.Expiration.HasValue)
            {
                props.Expiration = messageProperties.Expiration.Value.ToString(CultureInfo.InvariantCulture);
            }

            props.Headers = messageProperties.Headers;
            AttachCustomHeader(props);

            return props;
        }

        private static void AttachCustomHeader(IBasicProperties basicProperties)
        {
            if (basicProperties.Headers == null) basicProperties.Headers = new Dictionary<string, object>();
            basicProperties.Headers.Add(CustomHeader);
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