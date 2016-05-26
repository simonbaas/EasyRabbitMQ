using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Logging;
using EasyRabbitMQ.Serialization;
using RabbitMQ.Client;
using ExchangeType = EasyRabbitMQ.Infrastructure.ExchangeType;

namespace EasyRabbitMQ.Subscribe
{
    internal class ExchangeSubscription : AbstractSubscription
    {
        private string _queue;
        private IModel _channel;

        public ExchangeSubscription(Channel channel, ISerializer serializer, ILoggerFactory loggerFactory,
            string exchange, string routingKey, ExchangeType exchangeType) 
            : base(channel, serializer, loggerFactory)
        {
            Initialize(exchange, routingKey, exchangeType);
        }

        private void Initialize(string exchange, string routingKey, ExchangeType exchangeType)
        {
            var channel = Channel.Instance;

            channel.ExchangeDeclare(
                exchange: exchange,
                type: exchangeType.GetRabbitMQExchangeType(),
                durable: true,
                autoDelete: false,
                arguments: null);

            var queueOk = channel.QueueDeclare(
                queue: "",
                durable: false,
                exclusive: true,
                autoDelete: true,
                arguments: null);

            _queue = queueOk.QueueName;

            channel.QueueBind(
                queue: _queue,
                exchange: exchange,
                routingKey: routingKey);

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
