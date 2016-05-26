using System;

namespace EasyRabbitMQ.Configuration
{
    public static class EasyRabbitMQConfigure
    {
        public static EasyRabbitMQConfigurer ConnectTo(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));

            return new EasyRabbitMQConfigurer(connectionString);
        }
    }
}
