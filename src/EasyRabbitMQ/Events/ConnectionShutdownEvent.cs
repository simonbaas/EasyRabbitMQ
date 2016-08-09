using RabbitMQ.Client;

namespace EasyRabbitMQ.Events
{
    internal class ConnectionShutdownEvent
    {
        public IConnection Connection { get; }
        public ShutdownEventArgs ShutdownEventArgs { get; }

        public ConnectionShutdownEvent(IConnection connection, ShutdownEventArgs shutdownEventArgs)
        {
            Connection = connection;
            ShutdownEventArgs = shutdownEventArgs;
        }
    }
}
