using EasyRabbitMQ.Configuration;
using RabbitMQ.Client;

namespace EasyRabbitMQ.Infrastructure
{
    internal class ConnectionFactory : IConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public ConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConnection CreateConnection()
        {
            var factory = new RabbitMQ.Client.ConnectionFactory
            {
                Uri = _configuration.ConnectionString,
                AutomaticRecoveryEnabled = true
            };

            var connection = factory.CreateConnection();

            return connection;
        }
    }
}