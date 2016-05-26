using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Serialization;

namespace EasyRabbitMQ.Subscribe
{
    internal class SubscriptionFactory : ISubscriptionFactory
    {
        public IChannelFactory ChannelFactory { get; }
        public ISerializer Serializer { get; }

        internal SubscriptionFactory(IChannelFactory channelFactory, ISerializer serializer)
        {
            ChannelFactory = channelFactory;
            Serializer = serializer;
        }
    }
}