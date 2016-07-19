using System;
using Autofac;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Autofac
{
    public class AutofacHandlerActivator : IHandlerActivator
    {
        private readonly IContainer _container;

        public AutofacHandlerActivator(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            _container = container;
        }

        public IHandleMessages<TMessage> Get<TMessage>() where TMessage : class 
        {
            var lifetimeScope = _container.BeginLifetimeScope("autofac-handler-scope");

            return lifetimeScope.Resolve<IHandleMessages<TMessage>>();
        }

        public void Dispose()
        {
            
        }
    }
}
