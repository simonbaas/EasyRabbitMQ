using System;
using System.Linq;
using System.Threading.Tasks;
using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Logging;
using EasyRabbitMQ.Retry;
using EasyRabbitMQ.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EasyRabbitMQ.Subscribe
{
    internal abstract class AbstractAsyncSubscription<TMessage> : ISubscription<TMessage>, IStartable
    {
        public event Func<Message<TMessage>, Task> Received;

        protected Channel Channel { get; }
        protected ISerializer Serializer { get; }
        protected EventingBasicConsumer Consumer { get; private set; }

        private readonly IMessageHandlerActivator _activator;
        private readonly IMessageRetryHandler _messageRetryHandler;
        private readonly ILogger _logger;

        protected AbstractAsyncSubscription(Channel channel, ISerializer serializer, ILoggerFactory loggerFactory, 
            IMessageHandlerActivator activator, IMessageRetryHandler messageRetryHandler)
        {
            Channel = channel;
            Serializer = serializer;

            _activator = activator;
            _messageRetryHandler = messageRetryHandler;
            _logger = loggerFactory.GetLogger(typeof (AbstractAsyncSubscription<>));
        }

        protected abstract IModel GetChannel();
        protected abstract string GetQueue();

        public ISubscription<TMessage> AddHandler(Func<Message<TMessage>, Task> action)
        {
            return RegisterHandler(action);
        }

        public ISubscription<TMessage> AddHandler<THandler>() where THandler : IHandleMessagesAsync<TMessage>
        {
            var handler = _activator.Get<TMessage, THandler>();

            return RegisterHandler<TMessage>(handler.HandleAsync);
        }

        private ISubscription<TMessage> RegisterHandler<T>(Func<Message<T>, Task> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            Func<dynamic, Task> handler = message => action(message);

            Received += handler;

            return this;
        }

        public void Start()
        {
            var channel = GetChannel();
            var queue = GetQueue();

            if (channel == null) throw new InvalidOperationException("channel is invalid");
            if (string.IsNullOrWhiteSpace(queue)) throw new InvalidOperationException("queue is invalid");

            EnableFairDispatch(channel);

            Consumer = new EventingBasicConsumer(channel);
            Consumer.Received += ConsumerOnReceived;

            channel.BasicConsume(
                queue: queue,
                noAck: false,
                consumer: Consumer);
        }

        protected virtual async void ConsumerOnReceived(object sender, BasicDeliverEventArgs ea)
        {
            var consumer = (EventingBasicConsumer)sender;
            var channel = consumer.Model;

            try
            {
                var payload = Serializer.Deserialize<TMessage>(ea.Body);

                var message = MessageFactory.Create(ea, payload);

                await InvokeHandlersAsync(message).ConfigureAwait(false);

                channel.BasicAck(ea.DeliveryTag, false);

                return;
            }
            catch (Exception ex) when (_messageRetryHandler.ShouldRetryMessage(ea))
            {
                channel.BasicAck(ea.DeliveryTag, false);

                _logger.Error($"Failed to process message received from queue '{GetQueue()}'. Retrying message.", ex);

                _messageRetryHandler.RetryMessage(ea);

                return;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to process message received from queue '{GetQueue()}'.", ex);
            }

            channel.BasicNack(ea.DeliveryTag, false, false);
        }

        private async Task InvokeHandlersAsync(Message<TMessage> message)
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

        private static void EnableFairDispatch(IModel channel)
        {
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
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
                    if (Consumer != null)
                    {
                        Consumer.Received -= ConsumerOnReceived;
                    }

                    _messageRetryHandler?.Dispose();
                    Channel?.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}