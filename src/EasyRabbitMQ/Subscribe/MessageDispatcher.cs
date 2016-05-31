using System;
using System.Linq;
using System.Threading.Tasks;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    internal class MessageDispatcher<TMessage> : IMessageDispatcher<TMessage>
    {
        public event Func<Message<TMessage>, Task> Received;

        private readonly IHandlerActivator _activator;
        private readonly ITransactionContext _transactionContext = new TransactionContext();

        public MessageDispatcher(IHandlerActivator activator)
        {
            _activator = activator;
        }

        public void AddHandler(Func<Message<TMessage>, Task> action)
        {
            RegisterHandler(action);
        }

        public void AddHandler<THandler>() where THandler : IHandleMessagesAsync<TMessage>
        {
            var handler = _activator.Get<TMessage, THandler>(_transactionContext);

            if (handler == null) throw new InvalidOperationException($"Could not resolve handler of type '{typeof(THandler)}'.");

            RegisterHandler<TMessage>(handler.HandleAsync);
        }

        private void RegisterHandler<T>(Func<Message<T>, Task> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            Func<dynamic, Task> handler = message => action(message);

            Received += handler;
        }

        public async Task DispatchMessageAsync(Message<TMessage> message)
        {
            var handler = Received;
            if (handler != null)
            {
                foreach (var @delegate in handler.GetInvocationList().Cast<Func<Message<TMessage>, Task>>())
                {
                    await @delegate(message).ConfigureAwait(false);
                }
            }
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
                    _transactionContext?.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}