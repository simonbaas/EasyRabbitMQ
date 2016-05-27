using System;
using System.Threading.Tasks;

namespace EasyRabbitMQ.Subscribe
{
    internal interface ISubscription : IDisposable
    {
        event Func<dynamic, Task> Received;
        void Start();
    }
}