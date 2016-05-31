using System;
using System.Collections.Concurrent;

namespace EasyRabbitMQ.Infrastructure
{
    internal class BuiltInHandlerActivator : IHandlerActivator
    {
        private readonly ConcurrentDictionary<Type, dynamic> _types = new ConcurrentDictionary<Type, dynamic>(); 

        public THandler Get<TMessage, THandler>(ITransactionContext transactionContext) where THandler : IHandleMessagesAsync<TMessage>
        {
            if (transactionContext == null) throw new ArgumentNullException(nameof(transactionContext));

            var handler = _types.GetOrAdd(typeof (THandler), _ => Activator.CreateInstance<THandler>());

            transactionContext.OnDisposed(() =>
            {
                var disposable = handler as IDisposable;
                disposable?.Dispose();
            });

            return handler;
        }
    }
}