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
            container.Register<IChannelFactory, ChannelFactory>();
            container.Register<IMessageRetryHandlerFactory, MessageRetryHandlerFactory>();
            container.Register<ISerializer, DefaultJsonSerializer>();
            container.Register<IHandlerActivator, BuiltInHandlerActivator>();
            container.Register<ISubscriptionFactory, SubscriptionFactory>();
            container.Register<IPublisher, Publisher>();
            container.Register<ISubscriber, Subscriber>();
        }
    }
}
