using System;
using RabbitMQ.Client;

namespace EasyRabbitMQ.Infrastructure
{
    internal class Channel : IDisposable
    {
        internal IModel Instance { get; private set; }

        private IConnection _connection;

        public Channel(IConnection connection, IModel channel)
        {
            _connection = connection;

            Instance = channel;
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
                    if (Instance != null)
                    {
                        Instance.Close();
                        Instance.Dispose();
                        Instance = null;
                    }

                    if (_connection != null)
                    {
                        _connection.Close();
                        _connection.Dispose();
                        _connection = null;
                    }
                }

                _disposedValue = true;
            }
        }
    }
}
