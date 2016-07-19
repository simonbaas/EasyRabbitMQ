using System;

namespace EasyRabbitMQ.Events
{
    internal interface IEventBus
    {
        IDisposable Subscribe<T>(Action<T> handler) where T : class;
        void Publish<T>(T @event) where T : class;
    }
}
