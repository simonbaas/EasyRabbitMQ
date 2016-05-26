using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Logging;
using EasyRabbitMQ.Publish;
using EasyRabbitMQ.Serialization;
using EasyRabbitMQ.Subscribe;

namespace EasyRabbitMQ.Configuration
{
    public class EasyRabbitMQConfigurer
    {
        internal IChannelFactory ChannelFactory { get; }
        internal ISerializer Serializer { get; private set; }
        internal ILoggerFactory LoggerFactory { get; private set; }

        internal EasyRabbitMQConfigurer(string connectionString)
        {
            ChannelFactory = new ChannelFactory(new ConnectionFactory(connectionString));
            Serializer = new TypeUnawareJsonSerializer();
            LoggerFactory = new NullLoggerFactory();
        }

        public void Use(ISerializer serializer)
        {
            Serializer = serializer;
        }

        public void Use(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
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
