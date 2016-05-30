using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Logging;
using EasyRabbitMQ.Retry;
using EasyRabbitMQ.Serialization;
using RabbitMQ.Client;
using ExchangeType = EasyRabbitMQ.Infrastructure.ExchangeType;

namespace EasyRabbitMQ.Subscribe
{
    internal class ExchangeAsyncSubscription<T> : AbstractAsyncSubscription<T>
    {
        private string _queue;
        private IModel _channel;

        internal ExchangeAsyncSubscription(Channel channel, ISerializer serializer, ILoggerFactory loggerFactory, IMessageRetryHandler messageRetryHandler,
            string exchange, string queue, string routingKey, ExchangeType exchangeType) 
            : base(channel, serializer, loggerFactory, messageRetryHandler)
        {
            Initialize(exchange, queue, routingKey, exchangeType);
        }

        private void Initialize(string exchange, string queue, string routingKey, ExchangeType exchangeType)
        {
            var channel = Channel.Instance;

            channel.ExchangeDeclare(
                exchange: exchange,
                type: exchangeType.GetRabbitMQExchangeType(),
                durable: true,
                autoDelete: false,
                arguments: null);

            var queueExclusive = queue == "";
            var queueAutoDelete = queueExclusive;
            var queueDurable = !queueAutoDelete;

            var queueOk = channel.QueueDeclare(
                queue: queue,
                durable: queueDurable,
                exclusive: queueExclusive,
                autoDelete: queueAutoDelete,
                arguments: null);

            queue = queueOk.QueueName;

            channel.QueueBind(
                queue: queue,
                exchange: exchange,
                routingKey: routingKey);

            _queue = queue;
            _channel = channel;
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
