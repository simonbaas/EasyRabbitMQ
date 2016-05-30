using System;
using RabbitMQ.Client.Events;

namespace EasyRabbitMQ.Retry
{
    public interface IMessageRetryHandler : IDisposable
    {
        bool ShouldRetryMessage(BasicDeliverEventArgs ea);
        void RetryMessage(BasicDeliverEventArgs ea);
    }
}