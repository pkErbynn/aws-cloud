using MediatR;

namespace Sns.Customers.Consumer.Messages
{
    /// <summary>
    /// IRequest turns class to input for handlers
    /// </summary>
    public class CustomerCreated: IRequest, ISqsMessage
    {
        public required Guid Id { get; init; }
        public required string GithubUsername { get; init; }
        public required string FullName { get; init; }
        public required string Email { get; init; }
        public required DateTime DateOfBirth { get; init; }
    }

    public class CustomerUpdated: IRequest, ISqsMessage
    {
        public required Guid Id { get; init; }
        public required string GithubUsername { get; init; }
        public required string FullName { get; init; }
        public required string Email { get; init; }
        public required DateTime DateOfBirth { get; init; }
    }

    public class CustomerDeleted: IRequest, ISqsMessage
    {
        public required Guid Id { get; init; }
    }
}