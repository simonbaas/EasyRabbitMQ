# EasyRabbitMQ
A library to provide an easy api when working with RabbitMQ on .NET

## Installation
TBA

## Using
### Publish
#### Queue
```csharp
using (var publisher = EasyRabbitMQConfigure
    .ConnectTo("amqp://user:password@amqp.local")
    .AsPublisher())
    {
        var message = new { MessageId = Guid.NewGuid() };

        publisher.PublishQueue("test.queue", message);
    }
```
#### Exchange
```csharp
using (var publisher = EasyRabbitMQConfigure
    .ConnectTo("amqp://user:password@amqp.local")
    .AsPublisher())
    {
        var message = new { MessageId = Guid.NewGuid() };

        publisher.PublishExchange(
            exchange: "test",
            routingKey: "test",
            message: message);
    }
```
### Subscribe
#### Queue
```csharp
using (var subscriber = EasyRabbitMQConfigure
    .ConnectTo("amqp://user:password@amqp.local")
    .AsSubscriber())
    {
        subscriber
            .Queue("test.queue")
            .HandleWith(message =>
            {
                Console.WriteLine("Message received: " + message.Payload.MessageId);
                return Task.FromResult(0);
            });
    }
```

#### Exchange
```csharp
using (var subscriber = EasyRabbitMQConfigure
    .ConnectTo("amqp://user:password@amqp.local")
    .AsSubscriber())
    {
        subscriber
            .Exchange(
                exchange: "test.exchange",
                queue: "test.queue",
                routingKey: "*")
            .HandleWith(message =>
            {
                Console.WriteLine("Message received: " + message.Payload.MessageId);
                return Task.FromResult(0);
            });
    }
```
