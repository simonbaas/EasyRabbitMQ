using EasyRabbitMQ.Infrastructure;

namespace EasyRabbitMQ.Retry
{
    internal class MessageRetryHandlerFactory : IMessageRetryHandlerFactory
    {
        private readonly IChannelFactory _channelFactory;

        internal MessageRetryHandlerFactory(IChannelFactory channelFactory)
        {
            _channelFactory = channelFactory;
        }

        public IMessageRetryHandler CreateHandler()
        {
            var channel = _channelFactory.CreateChannel();
            return new MessageRetryHandler(channel);
        }
    }
}