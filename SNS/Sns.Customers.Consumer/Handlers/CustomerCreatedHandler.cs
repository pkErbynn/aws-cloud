using MediatR;
using Sns.Customers.Consumer.Messages;

namespace Sns.Customers.Consumer.Handlers
{
    /// <summary>
    /// IRequestHandler<CustomerCreated> convert class to handler with T input (CustomerCreated)
    /// </summary>
    public class CustomerCreatedHandler : IRequestHandler<CustomerCreated>
    {
        private readonly ILogger<CustomerCreatedHandler> _logger;

        public CustomerCreatedHandler(ILogger<CustomerCreatedHandler> logger)
        {
            _logger = logger;
        }

        public Task<Unit> Handle(CustomerCreated request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(request.FullName);
            return Unit.Task;
        }
    }
}