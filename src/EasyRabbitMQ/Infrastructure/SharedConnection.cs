using System;
using System.IO;
using EasyRabbitMQ.Events;
using EasyRabbitMQ.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EasyRabbitMQ.Infrastructure
{
    internal class SharedConnection : ISharedConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IEventBus _eventBus;
        private IConnection _instance;

        private readonly ILogger _logger = LogManager.GetLogger(typeof(SharedConnection));

        private readonly object _lockObject = new object();

        public IConnection Connection
        {
            get
            {
                lock (_lockObject)
                {
                    if (_instance == null)
                    {
                        _instance = CreateConnection();
                    }

                    return _instance;
                }
            }
        }

        public SharedConnection(IConnectionFactory connectionFactory, IEventBus eventBus)
        {
            _connectionFactory = connectionFactory;
            _eventBus = eventBus;
        }

        private IConnection CreateConnection()
        {
            var connection = _connectionFactory.CreateConnection();
            connection.ConnectionBlocked += Connection_ConnectionBlocked;
            connection.ConnectionUnblocked += ConnectionOnConnectionUnblocked;
            connection.ConnectionShutdown += ConnectionOnConnectionShutdown;

            return connection;
        }

        private void Connection_ConnectionBlocked(object sender, ConnectionBlockedEventArgs connectionBlockedEventArgs)
        {
            _logger.Info($"Connection blocked. Reason: {connectionBlockedEventArgs.Reason}");

            _eventBus.Publish(new ConnectionBlockedEvent((IConnection)sender, connectionBlockedEventArgs));
        }

        private void ConnectionOnConnectionUnblocked(object sender, EventArgs eventArgs)
        {
            _logger.Info("Connection unblocked");

            _eventBus.Publish(new ConnectionUnblockedEvent((IConnection)sender));
        }

        private void ConnectionOnConnectionShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            _logger.Info("Connection shutdown.");

            _eventBus.Publish(new ConnectionShutdownEvent((IConnection)sender, shutdownEventArgs));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        if (_instance != null)
                        {
                            _instance.ConnectionBlocked += Connection_ConnectionBlocked;
                            _instance.ConnectionUnblocked += ConnectionOnConnectionUnblocked;
                            _instance.ConnectionShutdown += ConnectionOnConnectionShutdown;
                            _instance.Dispose();
                            _instance = null;
                        }
                    }
                    catch (IOException ex)
                    {
                        _logger.Debug("An IOException occurred while disposing RabbitMQ connection.", ex);
                    }
                }

                _disposedValue = true;
            }
        }
    }
}