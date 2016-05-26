namespace EasyRabbitMQ.Infrastructure
{
    internal interface IChannelFactory
    {
        Channel CreateChannel();
    }
}
