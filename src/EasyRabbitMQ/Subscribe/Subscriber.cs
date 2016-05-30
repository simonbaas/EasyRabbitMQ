using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using EasyRabbitMQ.Configuration;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    internal class Subscriber : ISubscriber
    {
        private readonly ISubscriptionFactory _subscriptionFactory;
        private readonly ConcurrentDictionary<string, IStartable> _subscriptions = new ConcurrentDictionary<string, IStartable>();
        private readonly object _startLock = new object();
        private bool _isStarted;

        internal Subscriber(EasyRabbitMQConfigurer configurer)
        {
            _subscriptionFactory = new SubscriptionFactory(configurer.ChannelFactory, configurer.Serializer, configurer.LoggerFactory,
                configurer.MessageRetryHandler);
        }

        public IDisposable SubscribeQueue<T>(string queue, Func<Message<T>, Task> action)
        {
            CheckStarted();

            var subscription =  _subscriptions.GetOrAdd(queue, _ => _subscriptionFactory.SubscribeQueue<T>(queue)) as ISubscription<T>;

            return Subscribe(subscription, action);
        }

        public IDisposable SubscribeExchange<T>(string exchange, Func<Message<T>, Task> action, string queue = "",
            string routingKey = "", ExchangeType exchangeType = ExchangeType.Topic)
        {
            CheckStarted();

            var key = $"{exchange}:{queue}:{routingKey}:{exchangeType}";
            var subscription = _subscriptions.GetOrAdd(key, _ =>
                _subscriptionFactory.SubscribeExchange<T>(exchange, queue, routingKey, exchangeType)) as ISubscription<T>;

            return Subscribe(subscription, action);
        }

        public IDisposable SubscribeQueue(string queue, Func<Message<dynamic>, Task> action)
        {
            return SubscribeQueue<dynamic>(queue, action);
        }

        public IDisposable SubscribeExchange(string exchange, Func<Message<dynamic>, Task> action, string queue = "",
            string routingKey = "", ExchangeType exchangeType = ExchangeType.Topic)
        {
            return SubscribeExchange<dynamic>(exchange, action, queue, routingKey, exchangeType);
        }

        public void Start()
        {
            lock (_startLock)
            {
                if (_isStarted) return;

                if (_subscriptions == null || !_subscriptions.Any())
                {
                    throw new InvalidOperationException("Subscriber cannot be started because there are no subscriptions added.");
                }

                foreach (var subscription in _subscriptions)
                {
                    subscription.Value.Start();
                }

                _isStarted = true;
            }
        }

        private void CheckStarted()
        {
            lock (_startLock)
            {
                if (_isStarted) throw new InvalidOperationException("Subscriber is already started!");
            }
        }

        private static IDisposable Subscribe<T>(ISubscription<T> subscription, Func<Message<T>, Task> action)
        {
            Func<dynamic, Task> handler = message => action(message);

            subscription.Received += handler;

            return new DisposableAction(() => subscription.Received -= handler);
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
                    if (_subscriptions != null && _subscriptions.Any())
                    {
                        foreach (var subscription in _subscriptions)
                        {
                            subscription.Value.Dispose();
                        }

                        _subscriptions.Clear();
                    }
                }

                _disposedValue = true;
            }
        }
    }
}