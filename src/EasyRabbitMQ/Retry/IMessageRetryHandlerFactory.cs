namespace EasyRabbitMQ.Retry
{
    internal interface IMessageRetryHandlerFactory
    {
        IMessageRetryHandler CreateHandler();
    }
}
