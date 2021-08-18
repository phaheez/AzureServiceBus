using System.Threading.Tasks;

namespace SampleShared.Services
{
    public interface IQueueService
    {
        string ConnectionString { get; set; }
        string QueueName { get; set; }
        Task SendMessageAsync(string payload);
    }
}
