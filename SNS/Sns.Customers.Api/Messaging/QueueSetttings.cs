namespace Sns.Customers.Api.Messaging
{
    public class TopicSetttings
    {
        public const string Key = "TopicSettings";  // for section key
        public required string TopicName { get; set; }
    }
}