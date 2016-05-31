using System;
using System.Threading.Tasks;

namespace EasyRabbitMQ.Infrastructure
{
    internal interface IMessageDispatcher<TMessage> : IDisposable
    {
        event Func<Message<TMessage>, Task> Received;
        void AddHandler(Func<Message<TMessage>, Task> action);
        void AddHandler<THandler>() where THandler : IHandleMessages<TMessage>;
        Task DispatchMessageAsync(Message<TMessage> message);
    }
}
