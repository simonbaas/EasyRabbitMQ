using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Logging;
using EasyRabbitMQ.Retry;
using EasyRabbitMQ.Serialization;
using RabbitMQ.Client;

namespace EasyRabbitMQ.Subscribe
{
    internal class QueueAsyncSubscription<T> : AbstractAsyncSubscription<T>
    {
        private readonly string _queue;
        private IModel _channel;

        internal QueueAsyncSubscription(Channel channel, ISerializer serializer, ILoggerFactory loggerFactory, IMessageRetryHandler messageRetryHandler,
            string queue)
            : base(channel, serializer, loggerFactory, messageRetryHandler)
        {
            _queue = queue;

            Initialize();
        }

        private void Initialize()
        {
            _channel = Channel.Instance;

            _channel.QueueDeclare(
                queue: _queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        protected override IModel GetChannel()
        {
            return _channel;
        }

        protected override string GetQueue()
        {
            return _queue;
        }
    }
}