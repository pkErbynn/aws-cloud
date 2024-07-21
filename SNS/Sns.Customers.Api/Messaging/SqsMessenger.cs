using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Options;

/// <summary>
/// SQS Integration into API
/// </summary>
namespace Sns.Customers.Api.Messaging
{
    public class SnsMessenger: ISnsMessenger
    {
        private readonly IAmazonSimpleNotificationService _sns;
        private readonly IOptions<TopicSetttings> _topicSetttings;
        private string? _topicArn;  // Arn = Amazon Resource Name

        public SnsMessenger(IAmazonSimpleNotificationService sns, IOptions<TopicSetttings> topicSettings)
        {
            _sns = sns;
            _topicSetttings = topicSettings;
        }

        public async Task<PublishResponse> PublishMessageAsync<T>(T message)
        {
            var topicArn = await GetTopicArnAsync();

            var PublishRequest = new PublishRequest
            {
                TopicArn = topicArn,
                Message = JsonSerializer.Serialize(message),
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

            var messageResponse = await _sns.PublishAsync(PublishRequest);
            return messageResponse;
        }

        private async ValueTask<string> GetTopicArnAsync()   // use TaskValue instead of Task because, task is done only ones and the rest comes from cached value
        {
            if (_topicArn is not null)
                return _topicArn;

            var topicArnResponse = await _sns.FindTopicAsync(_topicSetttings.Value.TopicName);
            _topicArn = topicArnResponse.TopicArn;
            return _topicArn;
        }
    }
}