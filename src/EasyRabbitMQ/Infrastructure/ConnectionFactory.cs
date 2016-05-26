using RabbitMQ.Client;

namespace EasyRabbitMQ.Infrastructure
{
    internal class ConnectionFactory : IConnectionFactory
    {
        private readonly string _connectionString;

        internal ConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IConnection CreateConnection()
        {
            var factory = new RabbitMQ.Client.ConnectionFactory
            {
                Uri = _connectionString,
                AutomaticRecoveryEnabled = true
            };

            return factory.CreateConnection();
        }
    }
}