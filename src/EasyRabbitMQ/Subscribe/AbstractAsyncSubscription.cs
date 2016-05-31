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

        private readonly ISerializer _serializer;
        private readonly IMessageDispatcher<TMessage> _messageDispatcher; 
        private readonly IMessageRetryHandler _messageRetryHandler;
        private readonly ILogger _logger = LogManager.GetLogger(typeof (AbstractAsyncSubscription<>));
        private EventingBasicConsumer _consumer;

        protected AbstractAsyncSubscription(Channel channel, ISerializer serializer, IMessageDispatcher<TMessage> messageDispatcher, 
            IMessageRetryHandler messageRetryHandler)
        {
            Channel = channel;

            _serializer = serializer;
            _messageDispatcher = messageDispatcher;
            _messageRetryHandler = messageRetryHandler;
        }

        protected abstract string GetQueueName();

        public ISubscription<TMessage> HandleWith(Func<Message<TMessage>, Task> action)
        {
            _messageDispatcher.AddHandler(action);

            return this;
        }

        public ISubscription<TMessage> HandleWith<THandler>() where THandler : IHandleMessages<TMessage>
        {
            _messageDispatcher.AddHandler<THandler>();

            return this;
        }

        public void Start()
        {
            var channel = Channel.Instance;
            var queueName = GetQueueName();

            if (channel == null) throw new InvalidOperationException("channel is invalid");
            if (string.IsNullOrWhiteSpace(queueName)) throw new InvalidOperationException("queueName is invalid");

            EnableFairDispatch(channel);

            _consumer = new EventingBasicConsumer(channel);
            _consumer.Received += ConsumerOnReceived;

            channel.BasicConsume(
                queue: queueName,
                noAck: false,
                consumer: _consumer);
        }

        private async void ConsumerOnReceived(object sender, BasicDeliverEventArgs ea)
        {
            var consumer = (EventingBasicConsumer)sender;
            var channel = consumer.Model;

            try
            {
                var payload = _serializer.Deserialize<TMessage>(ea.Body);

                var message = MessageFactory.Create(ea, payload);

                await _messageDispatcher.DispatchMessageAsync(message).ConfigureAwait(false);

                channel.BasicAck(ea.DeliveryTag, false);

                return;
            }
            catch (Exception ex) when (_messageRetryHandler.ShouldRetryMessage(ea))
            {
                channel.BasicAck(ea.DeliveryTag, false);

                _logger.Error($"Failed to process message received from queue '{GetQueueName()}'. Retrying message.", ex);

                _messageRetryHandler.RetryMessage(ea);

                return;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to process message received from queue '{GetQueueName()}'.", ex);
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
                    if (_consumer != null)
                    {
                        _consumer.Received -= ConsumerOnReceived;
                        _consumer = null;
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