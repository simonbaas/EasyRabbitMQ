using System;
using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Logging;
using EasyRabbitMQ.Publish;
using EasyRabbitMQ.Serialization;
using EasyRabbitMQ.Subscribe;

namespace EasyRabbitMQ.Configuration
{
    public class EasyRabbitMQConfigurer
    {
        private readonly IContainer _container =  new SuperSimpleIoC();

        internal EasyRabbitMQConfigurer(string connectionString)
        {
            ComponentRegistration.Register(_container);

            _container.Register<IConnectionFactory>(() => new ConnectionFactory(connectionString));
        }

        public EasyRabbitMQConfigurer Use(ISerializer serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            _container.Register(() => serializer);

            return this;
        }

        public EasyRabbitMQConfigurer Use(IHandlerActivator handlerActivator)
        {
            if (handlerActivator == null) throw new ArgumentNullException(nameof(handlerActivator));

            _container.Register(() => handlerActivator);

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
            return _container.Resolve<IPublisher>();
        }

        public ISubscriber AsSubscriber()
        {
            return _container.Resolve<ISubscriber>();
        }
    }
}
