using System;
using System.Threading.Tasks;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    public interface ISubscription<TMessage>
    {
        ISubscription<TMessage> EnableRetry(int numberOfRetries);
        ISubscription<TMessage> UseFailureQueue(string failureQueue);

        ISubscription<TMessage> HandleWith(Func<Message<TMessage>, Task> action);
        ISubscription<TMessage> HandleWith<THandler>() where THandler : IHandleMessages<TMessage>;
    }
}