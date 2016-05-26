namespace EasyRabbitMQ.Infrastructure
{
    internal class ChannelFactory : IChannelFactory
    {
        private readonly IConnectionFactory _connectionFactory;
        public ChannelFactory(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public Channel CreateChannel()
        {
            var connection = _connectionFactory.CreateConnection();
            var channel = connection.CreateModel();

            return new Channel(connection, channel);
        }
    }
}