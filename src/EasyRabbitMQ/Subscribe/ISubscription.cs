using System;
using System.Threading.Tasks;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    public interface ISubscription<TMessage>
    {
        ISubscription<TMessage> AddHandler(Func<Message<TMessage>, Task> action);
        ISubscription<TMessage> AddHandler<THandler>() where THandler : IHandleMessagesAsync<TMessage>;
    }
}