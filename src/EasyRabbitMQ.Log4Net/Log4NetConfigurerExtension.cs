using EasyRabbitMQ.Configuration;

namespace EasyRabbitMQ.Log4Net
{
    public static class Log4NetConfigurerExtension
    {
        public static EasyRabbitMQConfigurer UseLog4Net(this EasyRabbitMQConfigurer configurer)
        {
            return configurer.Use(new Log4NetLoggerFactory());
        }
    }
}
