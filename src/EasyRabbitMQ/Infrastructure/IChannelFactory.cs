using System;

namespace EasyRabbitMQ.Infrastructure
{
    internal interface IChannelFactory : IDisposable
    {
        Channel CreateChannel();
    }
}
