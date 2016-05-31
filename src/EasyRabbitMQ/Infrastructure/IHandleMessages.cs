using System.Threading.Tasks;

namespace EasyRabbitMQ.Infrastructure
{
    public interface IHandleMessages<T>
    {
        Task HandleAsync(Message<T> message);
    }
}
