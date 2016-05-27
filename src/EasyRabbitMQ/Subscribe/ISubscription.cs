using System;
using System.Threading.Tasks;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    internal interface ISubscription : IDisposable
    {
        event Func<Message, Task> Received;
        void Start();
    }
}