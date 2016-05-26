namespace EasyRabbitMQ.Serialization
{
    public interface ISerializer
    {
        string ContentType { get; }

        object Deserialize(byte[] bytes);
        byte[] Serialize<T>(T obj);
    }
}