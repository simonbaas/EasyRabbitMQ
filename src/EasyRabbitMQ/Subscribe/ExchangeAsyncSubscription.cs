using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Retry;
using EasyRabbitMQ.Serialization;

namespace EasyRabbitMQ.Subscribe
{
    internal class ExchangeAsyncSubscription<TMessage> : AbstractAsyncSubscription<TMessage>
    {
        private string _queueName;

        internal ExchangeAsyncSubscription(Channel channel, ISerializer serializer, IMessageDispatcher<TMessage> messageDispatcher, 
            IMessageRetryHandler messageRetryHandler, string exchange, string queueName, string routingKey, ExchangeType exchangeType) 
            : base(channel, serializer, messageDispatcher, messageRetryHandler)
        {
            Initialize(exchange, queueName, routingKey, exchangeType);
        }

        private void Initialize(string exchange, string queueName, string routingKey, ExchangeType exchangeType)
        {
            var channel = Channel.Instance;

            channel.ExchangeDeclare(
                exchange: exchange,
                type: exchangeType.GetRabbitMQExchangeType(),
                durable: true,
                autoDelete: false,
                arguments: null);

            var queueExclusive = queueName == "";
            var queueAutoDelete = queueExclusive;
            var queueDurable = !queueAutoDelete;

            var queueOk = channel.QueueDeclare(
                queue: queueName,
                durable: queueDurable,
                exclusive: queueExclusive,
                autoDelete: queueAutoDelete,
                arguments: null);

            queueName = queueOk.QueueName;

            channel.QueueBind(
                queue: queueName,
                exchange: exchange,
                routingKey: routingKey);

            _queueName = queueName;
        }

        protected override string GetQueueName()
        {
            return _queueName;
        }
    }
}
