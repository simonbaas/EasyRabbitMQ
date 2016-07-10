namespace EasyRabbitMQ.Infrastructure
{
    public interface IHandlerActivator
    {
        IHandleMessages<TMessage> Get<TMessage>(ITransactionContext transactionContext) where TMessage : class;
    }
}
