using RabbitMQ.Client;

namespace EasyRabbitMQ.Events
{
    internal class ConnectionUnblockedEvent
    {
        public IConnection Connection { get; }

        public ConnectionUnblockedEvent(IConnection connection)
        {
            Connection = connection;
        }
    }
}
