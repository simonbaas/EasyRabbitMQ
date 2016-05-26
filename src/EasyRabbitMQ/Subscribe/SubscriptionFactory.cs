using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Logging;
using EasyRabbitMQ.Serialization;

namespace EasyRabbitMQ.Subscribe
{
    internal class SubscriptionFactory : ISubscriptionFactory
    {
        public IChannelFactory ChannelFactory { get; }
        public ISerializer Serializer { get; }
        public ILoggerFactory LoggerFactory { get; }

        internal SubscriptionFactory(IChannelFactory channelFactory, ISerializer serializer, ILoggerFactory loggerFactory)
        {
            ChannelFactory = channelFactory;
            Serializer = serializer;
            LoggerFactory = loggerFactory;
        }
    }
}