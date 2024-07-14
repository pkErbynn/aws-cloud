using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;

/// <summary>
/// SQS Integration into API
/// </summary>
namespace Customers.Api.Messaging
{
    public class SqsMessenger: ISqsMessenger
    {
        private readonly IAmazonSQS _sqs;
        private readonly IOptions<QueueSetttings> _queueSetttings;
        private string? _queueUrl;

        public SqsMessenger(IAmazonSQS sqs, IOptions<QueueSetttings> queueSettings)
        {
            _sqs = sqs;
            _queueSetttings = queueSettings;
        }

        public async Task<SendMessageResponse> SendMessageAsync<T>(T message)
        {
            var queueUrl = await GetQueueUrlAsync();

            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = JsonSerializer.Serialize(message),
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "MessageType", new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = typeof(T).Name
                        }
                    }
                }
            };

            var messageResponse = await _sqs.SendMessageAsync(sendMessageRequest);
            return messageResponse;
        }

        private async Task<string> GetQueueUrlAsync()
        {
            if (_queueUrl is not null)
                return _queueUrl;

            var queueUrlResponse = await _sqs.GetQueueUrlAsync(_queueSetttings.Value.QueueName);
            _queueUrl = queueUrlResponse?.QueueUrl;
            return _queueUrl;
        }
    }
}