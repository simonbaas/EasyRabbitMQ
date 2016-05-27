namespace EasyRabbitMQ.Serialization
{
    public interface ISerializer
    {
        string ContentType { get; }

        T Deserialize<T>(byte[] bytes);
        byte[] Serialize<T>(T obj);
    }
}