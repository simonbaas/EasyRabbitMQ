using System;
using System.Threading.Tasks;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    internal interface IMessageDispatcher<TMessage> : IDisposable
    {
        event Func<Message<TMessage>, Task> Received;
        void AddHandler(Func<Message<TMessage>, Task> action);
        void AddHandler<THandler>() where THandler : IHandleMessages<TMessage>;
        Task DispatchMessageAsync(Message<TMessage> message);
    }
}
