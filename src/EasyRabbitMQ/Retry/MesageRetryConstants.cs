namespace EasyRabbitMQ.Retry
{
    internal static class MesageRetryConstants
    {
        internal const string ExchangeHeaderKey = "x-easy-rabbitmq-exhange";
        internal const string RoutingKeyHeaderKey = "x-easy-rabbitmq-routingKey";
        internal const string RetriesHeaderKey = "x-easy-rabbitmq-retries";

        internal const string DeadLetterExchangeHeaderKey = "x-dead-letter-exchange";
        internal const string DeadLetterRoutingKeyHeaderKey = "x-dead-letter-routing-key";
        internal const string XDeathHeaderKey = "x-death";
        internal const string MessageTtlHeaderKey = "x-message-ttl";

        internal const string RetryExchange = "easy.rabbitmq.retry";
        internal const string RetryQueue = "easy.rabbitmq.retry";
        internal const string DelayedQueue = "easy.rabbitmq.delayed";

        internal const string RetryRoutingKey = "retry";

        internal const int RetryDelay = 5000;
    }
}
