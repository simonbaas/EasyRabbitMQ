using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EasyRabbitMQ.Events
{
    internal class ConnectionBlockedEvent
    {
        public IConnection Connection { get; }
        public ConnectionBlockedEventArgs ConnectionBlockedEventArgs { get; }

        public ConnectionBlockedEvent(IConnection connection, ConnectionBlockedEventArgs connectionBlockedEventArgs)
        {
            Connection = connection;
            ConnectionBlockedEventArgs = connectionBlockedEventArgs;
        }
    }
}
