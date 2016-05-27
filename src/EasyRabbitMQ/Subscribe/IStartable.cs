using System;

namespace EasyRabbitMQ.Subscribe
{
    internal interface IStartable : IDisposable
    {
        void Start();
    }
}