using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Events
{
    internal class EventBus : IEventBus
    {
        private readonly ConcurrentDictionary<Type, List<dynamic>> _subscriptions = new ConcurrentDictionary<Type, List<dynamic>>();
        private readonly object _lockObject = new object();

        public IDisposable Subscribe<T>(Action<T> handler) where T : class
        {
            var subscribers = _subscriptions.GetOrAdd(typeof(T), _ => new List<dynamic>());

            lock (_lockObject)
            {
                subscribers.Add(handler);
            }

            return new DisposableAction(() =>
            {
                lock (_lockObject)
                {
                    subscribers.Remove(handler);
                }
            });
        }

        public void Publish<T>(T @event) where T : class
        {
            List<dynamic> subscribers;
            if (!_subscriptions.TryGetValue(typeof(T), out subscribers)) return;

            lock (_lockObject)
            {
                foreach (var subscriber in subscribers)
                {
                    subscriber(@event);
                }
            }
        }
    }
}