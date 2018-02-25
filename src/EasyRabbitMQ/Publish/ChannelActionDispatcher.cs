using System;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMQ.Logging;
using RabbitMQ.Client;

namespace EasyRabbitMQ.Publish
{
    internal class ChannelActionDispatcher : IChannelActionDispatcher
    {
        private readonly IChannelConnection _channelConnection;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly ILogger _logger = LogManager.GetLogger(typeof(ChannelActionDispatcher));

        public ChannelActionDispatcher(IChannelConnection channelConnection)
        {
            _channelConnection = channelConnection;
        }

        public async Task InvokeAsync(Action<IModel> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            try
            {
                await _semaphore.WaitAsync(_cts.Token).ConfigureAwait(false);

                await Task
                    .Run(() => InvokeChannelAction(action), _cts.Token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Invoke(Action<IModel> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            InvokeAsync(action).Wait();
        }

        private void InvokeChannelAction(Action<IModel> action)
        {
            try
            {
                if (_cts.IsCancellationRequested) return;

                _channelConnection.InvokeActionOnChannel(action);
            }
            catch (Exception ex)
            {
                _logger.Error("An exception was thrown when invoking action on channel", ex);

                throw;
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
                    _cts.Cancel();
                    _channelConnection.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}