using System;
using System.Collections.Concurrent;

namespace EasyRabbitMQ.Infrastructure
{
    internal class BuiltInMessageHandlerActivator : IMessageHandlerActivator
    {
        private readonly ConcurrentDictionary<Type, dynamic> _types = new ConcurrentDictionary<Type, dynamic>(); 

        public THandler Get<TMessage, THandler>() where THandler : IHandleMessagesAsync<TMessage>
        {
            return _types.GetOrAdd(typeof (THandler), _ => Activator.CreateInstance<THandler>());
        }
    }
}