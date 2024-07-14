namespace Customers.Api.Messaging
{
    public class QueueSetttings
    {
        public const string Key = "QueueSettings";  // for section key
        public required string QueueName { get; set; }
    }
}