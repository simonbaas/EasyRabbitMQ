using System;
using System.Threading.Tasks;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    internal interface IReceiveMessages<TMessage>
    {
        event Func<Message<TMessage>, Task> Received;
    }
}
