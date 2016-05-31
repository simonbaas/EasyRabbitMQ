using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRabbitMQ.Extensions
{
    internal static class HeadersExtensions
    {
        internal static string GetString(this IDictionary<string, object> headers, string key)
        {
            if (headers == null) return null;
            if (!headers.ContainsKey(key)) return null;
            var bytes = headers[key] as byte[];
            return bytes == null ? null : Encoding.UTF8.GetString(bytes);
        }

        internal static T Get<T>(this IDictionary<string, object> headers, string key)
        {
            if (typeof(T) == typeof(string)) throw new InvalidOperationException("Use GetString method to get string value");

            if (headers == null) return default(T);
            if (!headers.ContainsKey(key)) return default(T);
            return (T) Convert.ChangeType(headers[key], typeof (T));
        }

        internal static void AddOrUpdate(this IDictionary<string, object> headers, string key, object value)
        {
            headers = headers ?? new Dictionary<string, object>();

            if (!headers.ContainsKey(key))
            {
                headers.Add(key, value);
            }
            else
            {
                headers[key] = value;
            }
        }
    }
}
