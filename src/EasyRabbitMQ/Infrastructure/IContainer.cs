using System;

namespace EasyRabbitMQ.Infrastructure
{
    internal interface IContainer
    {
        void Register<T, TU>() where TU : T;
        void Register<T>(Func<T> createInstance);
        T Resolve<T>();
    }
}