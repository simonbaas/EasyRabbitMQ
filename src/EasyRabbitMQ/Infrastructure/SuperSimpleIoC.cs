using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyRabbitMQ.Infrastructure
{
    internal class SuperSimpleIoC : IContainer
    {
        private readonly Dictionary<Type, RegisteredObject> _configItems = new Dictionary<Type, RegisteredObject>();

        public void Register<T, TU>() where TU : T
        {
            var typeToResolve = typeof (T);
            if (_configItems.ContainsKey(typeToResolve)) _configItems.Remove(typeToResolve);

            var configItem = new RegisteredObject
            {
                TypeToResolve = typeToResolve,
                ConcreteType = typeof(TU)
            };

            _configItems.Add(typeof(T), configItem);
        }

        public void Register<T>(Func<T> createInstance)
        {
            var typeToResolve = typeof(T);
            if (_configItems.ContainsKey(typeToResolve)) _configItems.Remove(typeToResolve);

            var instance = createInstance();
            var configItem = new RegisteredObject
            {
                TypeToResolve = typeToResolve,
                ConcreteType = instance.GetType(),
                Instance = instance
            };

            _configItems.Add(typeof(T), configItem);
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        private object Resolve(Type type)
        {
            if (!_configItems.ContainsKey(type)) return null;

            var configItem = _configItems[type];
            if (configItem.Instance != null) return configItem.Instance;

            var parameters = ResolveConstructorParameters(configItem.ConcreteType).ToArray();
            configItem.Instance = Activator.CreateInstance(configItem.ConcreteType, parameters);

            return configItem.Instance;
        }

        private IEnumerable<object> ResolveConstructorParameters(Type concreteType)
        {
            var constructorInfo = concreteType.GetConstructors().First();
            return constructorInfo.GetParameters().Select(parameterInfo => Resolve(parameterInfo.ParameterType));
        }

        private class RegisteredObject
        {
            public Type TypeToResolve { get; set; }
            public Type ConcreteType { get; set; }
            public dynamic Instance { get; set; }
        }
    }
}
