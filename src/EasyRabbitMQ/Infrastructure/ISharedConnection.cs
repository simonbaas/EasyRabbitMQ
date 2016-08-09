using System;
using RabbitMQ.Client;

namespace EasyRabbitMQ.Infrastructure
{
    internal interface ISharedConnection : IDisposable
    {
        IConnection Connection { get; }
    }
}
