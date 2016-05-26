using System;
using EasyRabbitMQ.Infrastructure;
using EasyRabbitMQ.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EasyRabbitMQ.Subscribe
{
    internal abstract class AbstractSubscription : ISubscription
    {
        public event Action<dynamic> Received;

        protected Channel Channel { get; }
        protected ISerializer Serializer { get; }
        protected EventingBasicConsumer Consumer { get; private set; }

        protected AbstractSubscription(Channel channel, ISerializer serializer)
        {
            Channel = channel;
            Serializer = serializer;
        }

        protected abstract IModel GetChannel();
        protected abstract string GetQueue();

        public void Start()
        {
            var channel = GetChannel();
            var queue = GetQueue();

            if (channel == null) throw new InvalidOperationException("channel is invalid");
            if (string.IsNullOrWhiteSpace(queue)) throw new InvalidOperationException("queue is invalid");

            EnableFairDispatch(channel);

            Consumer = new EventingBasicConsumer(channel);
            Consumer.Received += ConsumerOnReceived;

            channel.BasicConsume(
                queue: queue,
                noAck: false,
                consumer: Consumer);
        }

        protected virtual void ConsumerOnReceived(object sender, BasicDeliverEventArgs ea)
        {
            var consumer = (EventingBasicConsumer)sender;
            var channel = consumer.Model;

            try
            {
                dynamic message = Serializer.Deserialize(ea.Body);

                Received?.Invoke(message);

                channel.BasicAck(ea.DeliveryTag, false);

                return;
            }
            catch (Exception ex)
            {
                // TODO: Log!

            }

            channel.BasicNack(ea.DeliveryTag, false, false);
        }

        private static void EnableFairDispatch(IModel channel)
        {
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (Consumer != null)
                    {
                        Consumer.Received -= ConsumerOnReceived;
                    }

                    Channel?.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}