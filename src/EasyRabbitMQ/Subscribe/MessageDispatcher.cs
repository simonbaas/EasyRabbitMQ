using System;
using System.Linq;
using System.Threading.Tasks;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    internal class MessageDispatcher<TMessage> : IMessageDispatcher<TMessage>
    {
        private readonly IMessageHandlerActivator _activator;
        public event Func<Message<TMessage>, Task> Received;

        public MessageDispatcher(IMessageHandlerActivator activator)
        {
            _activator = activator;
        }

        public void AddHandler(Func<Message<TMessage>, Task> action)
        {
            RegisterHandler(action);
        }

        public void AddHandler<THandler>() where THandler : IHandleMessagesAsync<TMessage>
        {
            var handler = _activator.Get<TMessage, THandler>();

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
    }
}