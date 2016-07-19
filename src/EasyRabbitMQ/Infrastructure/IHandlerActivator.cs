using System;

namespace EasyRabbitMQ.Infrastructure
{
    public interface IHandlerActivator : IDisposable
    {
        IHandleMessages<TMessage> Get<TMessage>() where TMessage : class;
    }
}
