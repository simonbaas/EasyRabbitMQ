using System;
using System.Collections.Concurrent;
using System.Linq;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    internal class Subscriber : ISubscriber
    {
        private readonly ISubscriptionFactory _subscriptionFactory;
        private readonly ConcurrentDictionary<string, IStartable> _subscriptions = new ConcurrentDictionary<string, IStartable>();
        private readonly object _startLock = new object();
        private bool _isStarted;

        public Subscriber(ISubscriptionFactory subscriptionFactory)
        {
            _subscriptionFactory = subscriptionFactory;
        }

        public ISubscription<TMessage> Queue<TMessage>(string queue) where TMessage : class
        {
            CheckStarted();
            var key = $"{queue}-{typeof (TMessage).FullName}";
            var subscription = _subscriptions.GetOrAdd(key, _ => _subscriptionFactory.SubscribeQueue<TMessage>(queue)) as ISubscription<TMessage>;

            return subscription;
        }

        public ISubscription<TMessage> Exchange<TMessage>(string exchange, string queue = "", string routingKey = "",
            ExchangeType exchangeType = ExchangeType.Topic) where TMessage : class
        {
            CheckStarted();

            var key = $"{queue}-{exchange}-{typeof(TMessage).FullName}";
            var subscription = _subscriptions.GetOrAdd(key, 
                _ => _subscriptionFactory.SubscribeExchange<TMessage>(exchange, queue, routingKey, exchangeType)) as ISubscription<TMessage>;

            return subscription;
        }

        public ISubscription<dynamic> Queue(string queue)
        {
            return Queue<dynamic>(queue);
        }

        public ISubscription<dynamic> Exchange(string exchange, string queue = "", string routingKey = "",
            ExchangeType exchangeType = ExchangeType.Topic)
        {
            return Exchange<dynamic>(exchange, queue, routingKey, exchangeType);
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

                    _subscriptionFactory.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}