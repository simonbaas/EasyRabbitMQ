using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Retry
{
    internal class MessageRetryHandlerFactory : IMessageRetryHandlerFactory
    {
        private readonly IChannelFactory _channelFactory;

        public MessageRetryHandlerFactory(IChannelFactory channelFactory)
        {
            _channelFactory = channelFactory;
        }

        public IMessageRetryHandler CreateHandler()
        {
            return new MessageRetryHandler(_channelFactory);
        }
    }
}