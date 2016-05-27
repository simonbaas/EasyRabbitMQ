using System;
using System.Linq;
using System.Threading.Tasks;
using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Logging;
using EasyRabbitMQ.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EasyRabbitMQ.Subscribe
{
    internal abstract class AbstractAsyncSubscription<T> : ISubscription<T>
    {
        public event Func<Message<T>, Task> Received;

        protected Channel Channel { get; }
        protected ISerializer Serializer { get; }
        protected EventingBasicConsumer Consumer { get; private set; }

        private readonly ILogger _logger;

        protected AbstractAsyncSubscription(Channel channel, ISerializer serializer, ILoggerFactory loggerFactory)
        {
            Channel = channel;
            Serializer = serializer;

            _logger = loggerFactory.GetLogger(typeof (AbstractAsyncSubscription<>));
        }

        protected abstract IModel GetChannel();
        protected abstract string GetQueue();

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
                var payload = Serializer.Deserialize<T>(ea.Body);

                var message = MessageFactory.Create(ea, payload);

                await InvokeHandlersAsync(message).ConfigureAwait(false);

                channel.BasicAck(ea.DeliveryTag, false);

                return;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to process message received from queue '{GetQueue()}'", ex);
            }

            channel.BasicNack(ea.DeliveryTag, false, false);
        }

        private async Task InvokeHandlersAsync(Message<T> message)
        {
            var handler = Received;
            if (handler != null)
            {
                foreach (var @delegate in handler.GetInvocationList().Cast<Func<Message<T>, Task>>())
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

                    Channel?.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}