using System;
using EasyRabbitMQ.Infrastructure;
using Ninject;

namespace EasyRabbitMQ.Ninject
{
    public class NinjectHandlerActivator : IHandlerActivator
    {
        private readonly IKernel _kernel;

        public NinjectHandlerActivator(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            _kernel = kernel;
        }

        public THandler Get<TMessage, THandler>(ITransactionContext transactionContext) where THandler : IHandleMessages<TMessage>
        {
            if (transactionContext == null) throw new ArgumentNullException(nameof(transactionContext));

            var block = _kernel.BeginBlock();

            transactionContext.OnDisposed(() => block.Dispose());

            return block.Get<THandler>();
        }
    }
}
