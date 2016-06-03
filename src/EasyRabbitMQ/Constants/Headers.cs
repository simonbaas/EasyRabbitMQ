namespace EasyRabbitMQ.Constants
{
    internal class Headers
    {
        internal static readonly string HeaderPrefix = "x-easy-rabbitmq";
        internal static readonly string MessageId = $"{HeaderPrefix}-msg-id";
        internal static readonly string CorrelationId = $"{HeaderPrefix}-correlation-id";

        internal static readonly string Exchange = $"{HeaderPrefix}-exhange";
        internal static readonly string RoutingKey = $"{HeaderPrefix}-routingKey";
        internal static readonly string Retries = $"{HeaderPrefix}-retries";

        internal static readonly string XDeath = "x-death";
    }
}
