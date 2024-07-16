using Amazon.SQS;
using Customers.Consumer;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Queue consumer config
builder.Services.Configure<QueueSettings>(builder.Configuration.GetSection(QueueSettings.Key));
builder.Services.AddSingleton<IAmazonSQS, AmazonSQSClient>();
builder.Services.AddHostedService<QueueConsumerService>();  // Background service registered in DI

// Mediator config
builder.Services.AddMediatR(typeof(Program));


var app = builder.Build();



app.Run();
