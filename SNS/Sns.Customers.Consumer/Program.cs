using Amazon.SQS;
using MediatR;
using Sns.Customers.Consumer;

var builder = WebApplication.CreateBuilder(args);

// Queue consumer config
builder.Services.Configure<QueueSettings>(builder.Configuration.GetSection(QueueSettings.Key));
builder.Services.AddSingleton<IAmazonSQS, AmazonSQSClient>();
builder.Services.AddHostedService<QueueConsumerService>();  // Background service registered in DI

// Mediator config
builder.Services.AddMediatR(typeof(Program));


var app = builder.Build();



app.Run();
