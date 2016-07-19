using System;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace EasyRabbitMQ.Publish
{
    internal interface IChannelActionDispatcher : IDisposable
    {
        Task InvokeAsync(Action<IModel> action);
        void Invoke(Action<IModel> action);
    }
}
