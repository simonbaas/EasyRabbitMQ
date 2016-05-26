﻿using System;
using System.Collections.Generic;
using System.Linq;
using EasyRabbitMQ.Configuration;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    internal class Subscriber : ISubscriber
    {
        private readonly ISubscriptionFactory _subscriptionFactory;
        private readonly List<ISubscription> _subscriptions = new List<ISubscription>();
        private readonly object _startLock = new object();
        private bool _isStarted;

        internal Subscriber(EasyRabbitMQConfigurer configurer)
        {
            _subscriptionFactory = new SubscriptionFactory(configurer.ChannelFactory, configurer.Serializer);
        }

        public IDisposable On(string queue, Action<dynamic> action)
        {
            CheckStarted();

            var subscription = _subscriptionFactory.SubscribeQueue(queue);

            return Subscribe(subscription, action);
        }

        public IDisposable On(string exchange, string queue, string routingKey, ExchangeType exchangeType, Action<dynamic> action)
        {
            CheckStarted();

            var subscription = _subscriptionFactory.SubscribeExchange(exchange, queue, routingKey, exchangeType);

            return Subscribe(subscription, action);
        }

        public IDisposable On(string exchange, string routingKey, ExchangeType exchangeType, Action<dynamic> action)
        {
            CheckStarted();

            var subscription = _subscriptionFactory.SubscribeExchange(exchange, routingKey, exchangeType);

            return Subscribe(subscription, action);
        }

        public void Start()
        {
            lock (_startLock)
            {
                if (_isStarted) return;

                foreach (var subscription in _subscriptions)
                {
                    subscription.Start();
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

        private IDisposable Subscribe(ISubscription subscription, Action<dynamic> action)
        {
            Action<dynamic> handler = message =>
            {
                action(message);
            };

            subscription.Received += handler;

            _subscriptions.Add(subscription);

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
                            subscription.Dispose();
                        }

                        _subscriptions.Clear();
                    }
                }

                _disposedValue = true;
            }
        }
    }
}