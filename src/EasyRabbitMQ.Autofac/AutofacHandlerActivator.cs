using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public THandler Get<TMessage, THandler>(ITransactionContext transactionContext) where THandler : IHandleMessages<TMessage>
        {
            if (transactionContext == null) throw new ArgumentNullException(nameof(transactionContext));

            var lifetimeScope = _container.BeginLifetimeScope();

            transactionContext.OnDisposed(() => lifetimeScope.Dispose());

            return lifetimeScope.Resolve<THandler>();
        }
    }
}
