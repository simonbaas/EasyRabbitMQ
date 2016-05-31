using System;
using Autofac;
using EasyRabbitMQ.Configuration;

namespace EasyRabbitMQ.Autofac
{
    public static class AutofacConfigurerExtension
    {
        public static EasyRabbitMQConfigurer UseAutofac(this EasyRabbitMQConfigurer configurer, IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            configurer.Use(new AutofacHandlerActivator(container));

            return configurer;
        }
    }
}
