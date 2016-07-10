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

        public IHandleMessages<TMessage> Get<TMessage>(ITransactionContext transactionContext) where TMessage : class 
        {
            if (transactionContext == null) throw new ArgumentNullException(nameof(transactionContext));

            var block = _kernel.BeginBlock();

            transactionContext.OnDisposed(() => block.Dispose());

            return block.Get<IHandleMessages<TMessage>>();
        }
    }
}
