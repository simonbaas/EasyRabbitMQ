using System;

namespace EasyRabbitMQ.Logging
{
    public interface ILoggerFactory
    {
        ILogger GetLogger(Type type);
        ILogger GetLogger<T>();
    }
}
