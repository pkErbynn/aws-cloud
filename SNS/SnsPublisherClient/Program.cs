using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using SnsPublisherClient;

var snsClient = new AmazonSimpleNotificationServiceClient();

var customerCreated = new CustomerCreated
{
    Id = Guid.NewGuid(),
    Email = "john.erbynn@gmail.com",
    FullName = "john erbynn",
    DateOfBirth = new DateTime(1996, 1, 1),
    GithubUsername = "pkerbynn"
};

var topicArn = await snsClient.FindTopicAsync("customers"); // finding topic instead of queue url 

var publishRequest = new PublishRequest
{
    TopicArn = topicArn.TopicArn,
    Message = JsonSerializer.Serialize(customerCreated),
    MessageAttributes = new Dictionary<string, MessageAttributeValue>
    {
        { "MessageType", new MessageAttributeValue
            {
                DataType = "String",
                StringValue = nameof(customerCreated)
            } 
        }
    }
};

var response = await snsClient.PublishAsync(publishRequest);

Console.WriteLine($"PUBLISH RESPONSE:: {response.HttpStatusCode} - {response.MessageId}");


