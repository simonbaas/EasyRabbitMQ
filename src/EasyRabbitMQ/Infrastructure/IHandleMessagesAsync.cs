using System.Threading.Tasks;

namespace EasyRabbitMQ.Infrastructure
{
    public interface IHandleMessagesAsync<T>
    {
        Task HandleAsync(Message<T> message);
    }
}
