using System;
using RabbitMQ.Client;

namespace EasyRabbitMQ.Infrastructure
{
    internal interface IChannelFactory : IDisposable
    {
        IModel CreateChannel();
    }
}
