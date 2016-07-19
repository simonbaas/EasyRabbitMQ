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

        public IHandleMessages<TMessage> Get<TMessage>() where TMessage : class
        {
            dynamic handler;
            if (!_types.TryGetValue(typeof (TMessage), out handler))
            {
                throw new InvalidOperationException($"No handler registered for type '{typeof(TMessage)}'. Use Register method to register handlers.");
            }

            return handler;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var type in _types)
                    {
                        (type.Value as IDisposable)?.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }
    }
}