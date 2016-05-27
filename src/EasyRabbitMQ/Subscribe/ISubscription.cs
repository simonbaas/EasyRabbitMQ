using System;
using System.Threading.Tasks;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    internal interface ISubscription<T> : IStartable
    {
        event Func<Message<T>, Task> Received;
    }
}