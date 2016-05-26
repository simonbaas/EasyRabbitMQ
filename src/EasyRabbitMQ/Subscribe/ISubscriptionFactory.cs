using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Serialization;

namespace EasyRabbitMQ.Subscribe
{
    internal interface ISubscriptionFactory
    {
        IChannelFactory ChannelFactory { get; }
        ISerializer Serializer { get; }
    }
}