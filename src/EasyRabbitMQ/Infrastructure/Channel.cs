using System;
using RabbitMQ.Client;

namespace EasyRabbitMQ.Infrastructure
{
    internal class Channel : IDisposable
    {
        internal IModel Instance { get; private set; }

        private IConnection _connection;

        internal Channel(IConnection connection, IModel channel)
        {
            _connection = connection;

            Instance = channel;
        }

        internal void EnableFairDispatch()
        {
            Instance?.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
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
