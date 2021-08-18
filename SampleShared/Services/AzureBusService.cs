using Azure.Messaging.ServiceBus;
using System.Threading.Tasks;

namespace SampleShared.Services
{
    public class AzureBusService : IQueueService
    {
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
        public async Task SendMessageAsync(string payload)
        {
            // instantiate servicebusclient
            await using var client = new ServiceBusClient(ConnectionString);

            // create the sender
            ServiceBusSender sender = client.CreateSender(QueueName);

            // create a message that we can send. UTF-8 encoding is used when providing a string.
            ServiceBusMessage message = new ServiceBusMessage(payload);

            // send the message
            await sender.SendMessageAsync(message).ConfigureAwait(false);
        }
    }
}
