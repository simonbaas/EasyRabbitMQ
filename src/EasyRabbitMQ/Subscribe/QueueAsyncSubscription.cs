using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Logging;
using EasyRabbitMQ.Retry;
using EasyRabbitMQ.Serialization;
using RabbitMQ.Client;

namespace EasyRabbitMQ.Subscribe
{
    internal class QueueAsyncSubscription<TMessage> : AbstractAsyncSubscription<TMessage>
    {
        private readonly string _queueName;
        private IModel _channel;

        internal QueueAsyncSubscription(Channel channel, ISerializer serializer, ILoggerFactory loggerFactory,
            IMessageDispatcher<TMessage> messageDispatcher, IMessageRetryHandler messageRetryHandler,
            string queueName)
            : base(channel, serializer, loggerFactory, messageDispatcher, messageRetryHandler)
        {
            _queueName = queueName;

            Initialize(queueName);
        }

        private void Initialize(string queueName)
        {
            _channel = Channel.Instance;

            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        protected override string GetQueueName()
        {
            return _queueName;
        }
    }
}