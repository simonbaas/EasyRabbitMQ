using System;

namespace EasyRabbitMQ.Infrastructure
{
    internal class ChannelFactory : IChannelFactory
    {
        private readonly ISharedConnection _sharedConnection;

        public ChannelFactory(ISharedConnection sharedConnection)
        {
            _sharedConnection = sharedConnection;
        }

        public Channel CreateChannel()
        {
            var connection = _sharedConnection.Connection;
            var channel = connection.CreateModel();

            return new Channel(connection, channel);
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
                    _sharedConnection.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}