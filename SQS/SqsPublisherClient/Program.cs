using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using SqsPublisherClient;

var sqsClient = new AmazonSQSClient();

var customerCreated = new CustomerCreated
{
    Id = Guid.NewGuid(),
    Email = "john.erbynn@gmail.com",
    FullName = "john erbynn",
    DateOfBirth = new DateTime(1996, 1, 1),
    GithubUsername = "pkerbynn"
};

var queueUrl = await sqsClient.GetQueueUrlAsync("customers");

var sendMessageRequest = new SendMessageRequest
{
    QueueUrl = queueUrl.QueueUrl,
    MessageBody = JsonSerializer.Serialize(customerCreated),
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

var res = await sqsClient.SendMessageAsync(sendMessageRequest);

System.Console.WriteLine(res);