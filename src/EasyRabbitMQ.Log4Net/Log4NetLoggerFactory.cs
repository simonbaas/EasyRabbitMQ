using System;
using EasyRabbitMQ.Logging;

namespace EasyRabbitMQ.Log4Net
{
    public class Log4NetLoggerFactory : AbstractLoggerFactory
    {
        public override ILogger GetLogger(Type type)
        {
            return new Log4NetLogger(type);
        }
    }
}
