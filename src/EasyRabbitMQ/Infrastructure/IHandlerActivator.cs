namespace EasyRabbitMQ.Infrastructure
{
    public interface IHandlerActivator
    {
        THandler Get<TMessage, THandler>(ITransactionContext transactionContext) where THandler : IHandleMessagesAsync<TMessage>;
    }
}
