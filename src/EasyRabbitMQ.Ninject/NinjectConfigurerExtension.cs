using System;
using EasyRabbitMQ.Configuration;
using Ninject;

namespace EasyRabbitMQ.Ninject
{
    public static class NinjectConfigurerExtension
    {
        public static EasyRabbitMQConfigurer UseNinject(this EasyRabbitMQConfigurer configurer, IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            return configurer.Use(new NinjectHandlerActivator(kernel));
        }
    }
}
