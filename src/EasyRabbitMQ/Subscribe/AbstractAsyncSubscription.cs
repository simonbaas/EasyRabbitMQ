using System;
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
        protected Channel Channel { get; }
        protected ISerializer Serializer { get; }
        protected EventingBasicConsumer Consumer { get; private set; }

        private readonly IMessageDispatcher<TMessage> _messageDispatcher; 
        private readonly IMessageRetryHandler _messageRetryHandler;
        private readonly ILogger _logger;

        protected AbstractAsyncSubscription(Channel channel, ISerializer serializer, ILoggerFactory loggerFactory,
            IMessageDispatcher<TMessage> messageDispatcher, IMessageRetryHandler messageRetryHandler)
        {
            Channel = channel;
            Serializer = serializer;

            _messageDispatcher = messageDispatcher;
            _messageRetryHandler = messageRetryHandler;
            _logger = loggerFactory.GetLogger(typeof (AbstractAsyncSubscription<>));
        }

        protected abstract IModel GetChannel();
        protected abstract string GetQueue();

        public ISubscription<TMessage> AddHandler(Func<Message<TMessage>, Task> action)
        {
            _messageDispatcher.AddHandler(action);

            return this;
        }

        public ISubscription<TMessage> AddHandler<THandler>() where THandler : IHandleMessagesAsync<TMessage>
        {
            _messageDispatcher.AddHandler<THandler>();

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

        private async void ConsumerOnReceived(object sender, BasicDeliverEventArgs ea)
        {
            var consumer = (EventingBasicConsumer)sender;
            var channel = consumer.Model;

            try
            {
                var payload = Serializer.Deserialize<TMessage>(ea.Body);

                var message = MessageFactory.Create(ea, payload);

                await _messageDispatcher.DispatchMessageAsync(message).ConfigureAwait(false);

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
                    _messageDispatcher?.Dispose();
                    Channel?.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}