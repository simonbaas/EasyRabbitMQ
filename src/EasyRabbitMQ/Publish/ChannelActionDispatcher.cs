using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMQ.Logging;
using RabbitMQ.Client;

namespace EasyRabbitMQ.Publish
{
    internal class ChannelActionDispatcher : IChannelActionDispatcher
    {
        private const int QueueSize = 1;

        private readonly IChannelConnection _channelConnection;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly BlockingCollection<Action> _actions = new BlockingCollection<Action>(QueueSize);
        private readonly Task _dispatcherTask;

        private readonly ILogger _logger = LogManager.GetLogger(typeof(ChannelActionDispatcher));

        public ChannelActionDispatcher(IChannelConnection channelConnection)
        {
            _channelConnection = channelConnection;

            _dispatcherTask = StartDispatcherTask();
        }

        public Task InvokeAsync(Action<IModel> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            var tcs = new TaskCompletionSource<bool>();

            try
            {
                _actions.Add(() =>
                {
                    try
                    {
                        if (_cts.IsCancellationRequested)
                        {
                            tcs.SetCanceled();
                            return;
                        }

                        _channelConnection.InvokeActionOnChannel(action);

                        tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("An exception was thrown when invoking action on channel", ex);

                        tcs.SetException(ex);
                    }
                }, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                tcs.SetCanceled();
            }

            return tcs.Task;
        }

        public void Invoke(Action<IModel> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            InvokeAsync(action).Wait();
        }

        private Task StartDispatcherTask()
        {
            var task = Task.Run(() =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        var action = _actions.Take(_cts.Token);

                        action();
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.Debug("ChannelActionDispatcher task cancelled");

                        break;
                    }
                }
            }, _cts.Token);

            _logger.Debug("ChannelActionDispatcher task started");

            return task;
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
                    _dispatcherTask.Wait();
                    _channelConnection.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}