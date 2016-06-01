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
        internal IMessageRetryHandlerFactory MessageRetryHandlerFactory { get; private set; }
        internal ISerializer Serializer { get; private set; } = new DefaultJsonSerializer();
        internal IHandlerActivator HandlerActivator { get; private set; } = new BuiltInHandlerActivator();

        internal EasyRabbitMQConfigurer(string connectionString)
        {
            ChannelFactory = new ChannelFactory(new ConnectionFactory(connectionString));
            MessageRetryHandlerFactory = new MessageRetryHandlerFactory(ChannelFactory);
        }

        public EasyRabbitMQConfigurer Use(ISerializer serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            Serializer = serializer;

            return this;
        }

        public EasyRabbitMQConfigurer Use(IHandlerActivator handlerActivator)
        {
            if (handlerActivator == null) throw new ArgumentNullException(nameof(handlerActivator));

            HandlerActivator = handlerActivator;

            return this;
        }

        public EasyRabbitMQConfigurer Use(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            LogManager.LoggerFactory = loggerFactory;

            return this;
        }

        public IPublisher AsPublisher()
        {
            return new Publisher(this);
        }

        public ISubscriber AsSubscriber()
        {
            return new Subscriber(this);
        }
    }
}
