using System;
using System.Threading.Tasks;
using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Subscribe
{
    internal interface IMessageDispatcher<TMessage>
    {
        event Func<Message<TMessage>, Task> Received;
        void AddHandler(Func<Message<TMessage>, Task> action);
        void AddHandler<THandler>() where THandler : IHandleMessagesAsync<TMessage>;
        Task DispatchMessageAsync(Message<TMessage> message);
    }
}
