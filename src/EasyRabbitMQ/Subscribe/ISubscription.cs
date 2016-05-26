using System;

namespace EasyRabbitMQ.Subscribe
{
    internal interface ISubscription : IDisposable
    {
        event Action<dynamic> Received;
        void Start();
    }
}