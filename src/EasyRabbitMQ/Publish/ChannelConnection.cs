using System;
using EasyRabbitMQ.Infrastructure;
using RabbitMQ.Client;

namespace EasyRabbitMQ.Publish
{
    internal class ChannelConnection : IChannelConnection
    {
        private readonly IChannelFactory _channelFactory;
        private IModel _channel;
        private readonly object _lockObject = new object();

        public ChannelConnection(IChannelFactory channelFactory)
        {
            _channelFactory = channelFactory;
        }

        public void InvokeActionOnChannel(Action<IModel> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            try
            {
                var channel = GetChannel();

                action(channel);
            }
            catch
            {
                CloseChannel();

                throw;
            }
        }

        private IModel GetChannel()
        {
            lock (_lockObject)
            {
                if (_channel != null)
                {
                    return _channel;
                }

                _channel = _channelFactory.CreateChannel();

                return _channel;
            }
        }

        private void CloseChannel()
        {
            lock (_lockObject)
            {
                _channel?.Dispose();
                _channel = null;
            }
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
                    _channel?.Dispose();
                    _channel = null;

                    _channelFactory.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}