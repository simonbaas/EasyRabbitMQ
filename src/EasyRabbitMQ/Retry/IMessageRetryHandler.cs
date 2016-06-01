using System;
using RabbitMQ.Client.Events;

namespace EasyRabbitMQ.Retry
{
    internal interface IMessageRetryHandler : IDisposable
    {
        void SetMaxNumberOfRetries(int maxNumberOfRetries);
        bool ShouldRetryMessage(BasicDeliverEventArgs ea);
        void RetryMessage(BasicDeliverEventArgs ea);

        void SetFailureQueue(string failQueue);
        bool ShouldSendToFailureQueue();
        void SendToFailureQueue(BasicDeliverEventArgs ea);

        void Start();
    }
}