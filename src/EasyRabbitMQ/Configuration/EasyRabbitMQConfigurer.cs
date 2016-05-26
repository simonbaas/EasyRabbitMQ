using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Publish;
using EasyRabbitMQ.Serialization;
using EasyRabbitMQ.Subscribe;

namespace EasyRabbitMQ.Configuration
{
    public class EasyRabbitMQConfigurer
    {
        internal IChannelFactory ChannelFactory { get; }
        internal ISerializer Serializer { get; }

        internal EasyRabbitMQConfigurer(string connectionString)
        {
            ChannelFactory = new ChannelFactory(new ConnectionFactory(connectionString));
            Serializer = new TypeUnawareJsonSerializer();
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
