using System;
using System.Collections.Concurrent;

namespace EasyRabbitMQ.Infrastructure
{
    public class BuiltInHandlerActivator : IHandlerActivator
    {
        private readonly ConcurrentDictionary<Type, dynamic> _types = new ConcurrentDictionary<Type, dynamic>();

        public void Register<TMessage, THandler>(Func<THandler> func) where THandler : IHandleMessages<TMessage>
        {
            _types.AddOrUpdate(typeof (THandler), type => func(), (type, o) => func());
        }

        public THandler Get<TMessage, THandler>(ITransactionContext transactionContext) where THandler : IHandleMessages<TMessage>
        {
            if (transactionContext == null) throw new ArgumentNullException(nameof(transactionContext));

            dynamic handler;
            if (!_types.TryGetValue(typeof (THandler), out handler))
            {
                throw new InvalidOperationException($"No handler registered for type '{typeof(THandler)}'. Use Register method to register handlers.");
            }

            transactionContext.OnDisposed(() =>
            {
                var disposable = handler as IDisposable;
                disposable?.Dispose();
            });

            return handler;
        }
    }
}