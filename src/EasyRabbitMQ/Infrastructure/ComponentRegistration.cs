using EasyRabbitMQ.Configuration;
using EasyRabbitMQ.Publish;
using EasyRabbitMQ.Retry;
using EasyRabbitMQ.Serialization;
using EasyRabbitMQ.Subscribe;

namespace EasyRabbitMQ.Infrastructure
{
    internal class ComponentRegistration
    {
        internal static void Register(IContainer container)
        {
            container.Register<IConfiguration, Configuration.Configuration>();
            container.Register<IConnectionFactory, ConnectionFactory>();
            container.Register<IChannelFactory, ChannelFactory>();
            container.Register<ISerializer, DefaultJsonSerializer>();
            container.Register<IMessageRetryHandlerFactory, MessageRetryHandlerFactory>();
            container.Register<IHandlerActivator, BuiltInHandlerActivator>();
            container.Register<ISubscriptionFactory, SubscriptionFactory>();
            container.Register<ISubscriber, Subscriber>();
            container.Register<IChannelConnection, ChannelConnection>();
            container.Register<IChannelActionDispatcher, ChannelActionDispatcher>();
            container.Register<IPublisher, Publisher>();
        }
    }
}
