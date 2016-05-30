namespace EasyRabbitMQ.Infrastructure
{
    public interface IMessageHandlerActivator
    {
        THandler Get<TMessage, THandler>() where THandler : IHandleMessagesAsync<TMessage>;
    }
}
