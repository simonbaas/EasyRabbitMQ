using RabbitMQ.Client;

namespace EasyRabbitMQ.Infrastructure
{
    internal interface IConnectionFactory
    {
        IConnection CreateConnection();
    }
}
