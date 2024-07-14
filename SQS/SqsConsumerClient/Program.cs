using Amazon.SQS;
using Amazon.SQS.Model;

var sqsClient = new AmazonSQSClient();
var cts = new CancellationTokenSource();

var queueUrl = await sqsClient.GetQueueUrlAsync("customers");

var receiveMessageRequest = new ReceiveMessageRequest
{
    QueueUrl = queueUrl.QueueUrl,
    AttributeNames = new List<string> { "All" }, // show 'attribute' names attached to message by producer
    MessageAttributeNames = new List<string>{ "All" } // show 'message details' from aws dashboard
};


while(!cts.IsCancellationRequested)
{
    var response = await sqsClient.ReceiveMessageAsync(receiveMessageRequest, cts.Token);

    foreach (var message in response.Messages)
    {
        Console.WriteLine($"Message Id: {message.MessageId}");
        Console.WriteLine($"Body: {message.Body}");

        await sqsClient.DeleteMessageAsync(queueUrl.QueueUrl, message.ReceiptHandle, cts.Token);
    }

    await Task.Delay(1000);
}

// System.Console.WriteLine(response);