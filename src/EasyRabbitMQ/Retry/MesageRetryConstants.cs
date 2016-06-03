namespace EasyRabbitMQ.Retry
{
    internal static class MesageRetryConstants
    {
        internal static readonly string RetryRoutingKey = "retry";
        internal static readonly int RetryDelay = 5000;
    }
}
