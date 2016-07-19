﻿using System;
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

        public IHandleMessages<TMessage> Get<TMessage>() where TMessage : class
        {
            var block = _kernel.BeginBlock();

            return block.Get<IHandleMessages<TMessage>>();
        }

        public void Dispose()
        {
            
        }
    }
}
