using System;
using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Logging;
using EasyRabbitMQ.Publish;
using EasyRabbitMQ.Retry;
using EasyRabbitMQ.Serialization;
using EasyRabbitMQ.Subscribe;

namespace EasyRabbitMQ.Configuration
{
    public class EasyRabbitMQConfigurer
    {
        internal IChannelFactory ChannelFactory { get; }
        internal IMessageRetryHandler MessageRetryHandler { get; private set; }

        internal ISerializer Serializer { get; private set; } = new DefaultJsonSerializer();
        internal IMessageHandlerActivator MessageHandlerActivator { get; private set; } = new BuiltInMessageHandlerActivator();
        internal ILoggerFactory LoggerFactory { get; private set; } = new NullLoggerFactory();

        internal EasyRabbitMQConfigurer(string connectionString)
        {
            ChannelFactory = new ChannelFactory(new ConnectionFactory(connectionString));
        }

        public void Use(ISerializer serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            Serializer = serializer;
        }

        public void Use(IMessageHandlerActivator messageHandlerActivator)
        {
            if (messageHandlerActivator == null) throw new ArgumentNullException(nameof(messageHandlerActivator));

            MessageHandlerActivator = messageHandlerActivator;
        }

        public void Use(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            LoggerFactory = loggerFactory;
        }

        public IPublisher AsPublisher()
        {
            return new Publisher(this);
        }

        public ISubscriber AsSubscriber(int numberOfRetries = 0)
        {
            MessageRetryHandler = new MessageRetryHandler(ChannelFactory, LoggerFactory, numberOfRetries);

            return new Subscriber(this);
        }
    }
}
