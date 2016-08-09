using System;
using System.Threading.Tasks;
using EasyRabbitMQ.Extensions;
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
        protected IModel Channel { get; }

        private readonly ISerializer _serializer;
        private readonly IMessageDispatcher<TMessage> _messageDispatcher; 
        private readonly IMessageRetryHandler _messageRetryHandler;
        private readonly ILogger _logger = LogManager.GetLogger(typeof (AbstractAsyncSubscription<>));
        private EventingBasicConsumer _consumer;

        protected internal AbstractAsyncSubscription(IModel channel, ISerializer serializer, IMessageDispatcher<TMessage> messageDispatcher, 
            IMessageRetryHandler messageRetryHandler)
        {
            Channel = channel;

            Channel.EnableFairDispatch();

            _serializer = serializer;
            _messageDispatcher = messageDispatcher;
            _messageRetryHandler = messageRetryHandler;
        }

        protected abstract string GetQueueName();

        public ISubscription<TMessage> EnableRetry(int numberOfRetries)
        {
            _messageRetryHandler.SetMaxNumberOfRetries(numberOfRetries);

            return this;
        }

        public ISubscription<TMessage> UseFailureQueue(string failureQueue)
        {
            if (string.IsNullOrWhiteSpace(failureQueue)) throw new ArgumentNullException(nameof(failureQueue));

            _messageRetryHandler.SetFailureQueue(failureQueue);

            return this;
        }

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
            var queueName = GetQueueName();

            if (Channel == null) throw new InvalidOperationException("channel is invalid");
            if (string.IsNullOrWhiteSpace(queueName)) throw new InvalidOperationException("queueName is invalid");

            _messageRetryHandler.Start();

            _consumer = new EventingBasicConsumer(Channel);
            _consumer.Received += ConsumerOnReceived;

            Channel.BasicConsume(
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
            }
            catch (Exception ex) when (_messageRetryHandler.ShouldRetryMessage(ea))
            {
                channel.BasicAck(ea.DeliveryTag, false);

                _logger.Error($"Failed to process message received from queue '{GetQueueName()}'. Retrying message.", ex);

                _messageRetryHandler.RetryMessage(ea);
            }
            catch (Exception ex) when (_messageRetryHandler.ShouldSendToFailureQueue())
            {
                channel.BasicAck(ea.DeliveryTag, false);

                _logger.Error($"Failed to process message received from queue '{GetQueueName()}'. Message will be sent to failure queue.", ex);

                _messageRetryHandler.SendToFailureQueue(ea);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to process message received from queue '{GetQueueName()}'. Message could potentially be lost. " +
                              "Check dead letter queue if configured.", ex);

                channel.BasicNack(ea.DeliveryTag, false, false);
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