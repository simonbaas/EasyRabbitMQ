using RabbitMQ.Client;

namespace EasyRabbitMQ.Extensions
{
    internal static class IModelExtensions
    {
        internal static void EnableFairDispatch(this IModel channel)
        {
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        }
    }
}
