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
        private readonly ConcurrentDictionary<string, ISubscription> _subscriptions = new ConcurrentDictionary<string, ISubscription>();
        private readonly object _startLock = new object();
        private bool _isStarted;

        internal Subscriber(EasyRabbitMQConfigurer configurer)
        {
            _subscriptionFactory = new SubscriptionFactory(configurer.ChannelFactory, configurer.Serializer, configurer.LoggerFactory);
        }

        public IDisposable SubscribeQueue(string queue, Func<dynamic, Task> action)
        {
            CheckStarted();

            var subscription =  _subscriptions.GetOrAdd(queue, _ => _subscriptionFactory.SubscribeQueue(queue));

            return Subscribe(subscription, action);
        }

        public IDisposable SubscribeExchange(string exchange, Func<dynamic, Task> action)
        {
            return SubscribeExchange(exchange, "", ExchangeType.Fanout, action);
        }

        public IDisposable SubscribeExchange(string exchange, string queue, Func<dynamic, Task> action)
        {
            return SubscribeExchange(exchange, queue, "", ExchangeType.Fanout, action);
        }

        public IDisposable SubscribeExchange(string exchange, string queue, string routingKey, ExchangeType exchangeType, Func<dynamic, Task> action)
        {
            CheckStarted();

            var key = $"{exchange}:{queue}:{routingKey}:{exchangeType}";
            var subscription = _subscriptions.GetOrAdd(key, _ => 
                _subscriptionFactory.SubscribeExchange(exchange, queue, routingKey, exchangeType));

            return Subscribe(subscription, action);
        }

        public IDisposable SubscribeExchange(string exchange, string routingKey, ExchangeType exchangeType, Func<dynamic, Task> action)
        {
            CheckStarted();

            var key = $"{exchange}:{routingKey}:{exchangeType}";
            var subscription = _subscriptions.GetOrAdd(key, _ =>
                _subscriptionFactory.SubscribeExchange(exchange, routingKey, exchangeType));

            return Subscribe(subscription, action);
        }

        public void Start()
        {
            lock (_startLock)
            {
                if (_isStarted) return;

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

        private static IDisposable Subscribe(ISubscription subscription, Func<dynamic, Task> action)
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