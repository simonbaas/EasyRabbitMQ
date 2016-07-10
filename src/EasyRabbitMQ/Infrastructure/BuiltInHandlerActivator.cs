using System;
using System.Collections.Concurrent;

namespace EasyRabbitMQ.Infrastructure
{
    public class BuiltInHandlerActivator : IHandlerActivator
    {
        private readonly ConcurrentDictionary<Type, dynamic> _types = new ConcurrentDictionary<Type, dynamic>();

        public void Register<TMessage>(Func<IHandleMessages<TMessage>> func) where TMessage : class 
        {
            _types.AddOrUpdate(typeof (TMessage), _ => func(), (t, o) => func());
        }

        public IHandleMessages<TMessage> Get<TMessage>(ITransactionContext transactionContext) where TMessage : class
        {
            if (transactionContext == null) throw new ArgumentNullException(nameof(transactionContext));

            dynamic handler;
            if (!_types.TryGetValue(typeof (TMessage), out handler))
            {
                throw new InvalidOperationException($"No handler registered for type '{typeof(TMessage)}'. Use Register method to register handlers.");
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