
using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using MediatR;
using Microsoft.Extensions.Options;
using Sns.Customers.Consumer.Messages;

namespace Sns.Customers.Consumer
{
    public class QueueConsumerService : BackgroundService
    {
        private readonly IAmazonSQS _sqs;
        private readonly IOptions<QueueSettings> _queueSettings;
        private readonly IMediator _mediator;
        private readonly ILogger<QueueConsumerService> _logger;

        public QueueConsumerService(IAmazonSQS sqs, 
            IOptions<QueueSettings> queueSettings, 
            IMediator mediator,
            ILogger<QueueConsumerService> logger)
        {
            _sqs = sqs;
            _queueSettings = queueSettings;
            _mediator = mediator;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queueResponse = await _sqs.GetQueueUrlAsync(_queueSettings.Value.QueueName, stoppingToken);

            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueResponse.QueueUrl,
                AttributeNames = new List<string>{ "All" },
                MessageAttributeNames = new List<string> { "All" }
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                var response = await _sqs.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);
                foreach (var message in response.Messages)
                {
                    var messageType = message.MessageAttributes["MessageType"].StringValue;

                    /////// Handle and process data based on event Type
                    // switch (messageType)
                    // {
                    //     case nameof(CustomerCreated):
                    //         var createdMessage = JsonSerializer.Deserialize<CustomerCreated>(message.Body);
                    //         // process
                    //         break;
                    //     case nameof(CustomerUpdated):
                    //         break;
                    //     case nameof(CustomerDeleted):
                    //         break;
                    // }

                    ///// Better way is to use Handlers and process data based on event Type, leveraging MediatR
                    var type = Type.GetType($"Sns.Customers.Consumer.Messages.{messageType}");
                    if (type == null)
                    {
                        _logger.LogWarning($"Unknown message class type: {messageType}");
                        continue;
                    }

                    var typedMessage = (ISqsMessage)JsonSerializer.Deserialize(message.Body, type)!;

                    try
                    {
                        await _mediator.Send(typedMessage, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Message failed during processing");
                        continue;
                    }

                    await _sqs.DeleteMessageAsync(queueResponse.QueueUrl, message.ReceiptHandle);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}