using System;
using System.Collections.Concurrent;

namespace EasyRabbitMQ.Infrastructure
{
    public interface ITransactionContext : IDisposable
    {
        void OnDisposed(Action action);
    }

    public class TransactionContext : ITransactionContext
    {
        readonly ConcurrentQueue<Action> _onDisposedActions = new ConcurrentQueue<Action>();

        public void OnDisposed(Action action)
        {
            _onDisposedActions.Enqueue(action);
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
                    Action action;
                    while (_onDisposedActions.TryDequeue(out action))
                    {
                        action();
                    }
                }

                _disposedValue = true;
            }
        }
    }
}
