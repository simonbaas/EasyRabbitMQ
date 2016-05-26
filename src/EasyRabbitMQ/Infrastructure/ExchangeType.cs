using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRabbitMQ.Infrastructure
{
    public enum ExchangeType
    {
        Direct,
        Fanout,
        Headers,
        Topic,
    }

    internal static class ExchangeTypeExtensions
    {
        internal static string GetRabbitMQExchangeType(this ExchangeType exchangeType)
        {
            switch (exchangeType)
            {
                case ExchangeType.Direct:
                    return RabbitMQ.Client.ExchangeType.Direct;
                case ExchangeType.Fanout:
                    return RabbitMQ.Client.ExchangeType.Fanout;
                case ExchangeType.Headers:
                    return RabbitMQ.Client.ExchangeType.Headers;
                case ExchangeType.Topic:
                    return RabbitMQ.Client.ExchangeType.Topic;
                default:
                    throw new InvalidOperationException("Unknown exchange type");
            }
        }
    }
}
