using Azure.Messaging.ServiceBus;

namespace ServiceBus.Services
{
    public class AzureServiceBusService
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly string _queueName;

        public AzureServiceBusService(string connectionString, string queueName)
        {
            _client = new ServiceBusClient(connectionString);
            _queueName = queueName;
            _sender = _client.CreateSender(_queueName);
        }

        public async Task SendMessageAsync(string message)
        {
            ServiceBusMessage busMessage = new ServiceBusMessage(message);
            await _sender.SendMessageAsync(busMessage);
        }

        public async Task<string?> ReceiveMessageAsync()
        {
            ServiceBusReceiver receiver = _client.CreateReceiver(_queueName);
            ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();
            if (receivedMessage != null)
            {
                string body = receivedMessage.Body.ToString();
                await receiver.CompleteMessageAsync(receivedMessage);
                return body;
            }
            return null;
        }

        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}