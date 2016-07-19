using System;
using RabbitMQ.Client;

namespace EasyRabbitMQ.Publish
{
    internal interface IChannelConnection : IDisposable
    {
        void InvokeActionOnChannel(Action<IModel> action);
    }
}