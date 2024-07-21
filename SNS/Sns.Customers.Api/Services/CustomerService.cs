using Customers.Api.Contracts.Messages;
using FluentValidation;
using FluentValidation.Results;
using Sns.Customers.Api.Domain;
using Sns.Customers.Api.Mapping;
using Sns.Customers.Api.Messaging;
using Sns.Customers.Api.Repositories;

namespace Sns.Customers.Api.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IGitHubService _gitHubService;
    private readonly ISnsMessenger _snsMessenger;

    public CustomerService(ICustomerRepository customerRepository, 
        IGitHubService gitHubService, ISnsMessenger sqsMessenger)
    {
        _customerRepository = customerRepository;
        _gitHubService = gitHubService;
        _snsMessenger = sqsMessenger;
    }

    public async Task<bool> CreateAsync(Customer customer)
    {
        var existingUser = await _customerRepository.GetAsync(customer.Id);
        if (existingUser is not null)
        {
            var message = $"A user with id {customer.Id} already exists";
            throw new ValidationException(message, GenerateValidationError(nameof(Customer), message));
        }

        var isValidGitHubUser = await _gitHubService.IsValidGitHubUser(customer.GitHubUsername);
        if (!isValidGitHubUser)
        {
            var message = $"There is no GitHub user with username {customer.GitHubUsername}";
            throw new ValidationException(message, GenerateValidationError(nameof(customer.GitHubUsername), message));
        }
        
        var customerDto = customer.ToCustomerDto();
        var isCreated = await _customerRepository.CreateAsync(customerDto);
        if (isCreated)
        {
            // Sns messenger
            await _snsMessenger.PublishMessageAsync(customer.ToCustomerCreatedMessage());
        }
        return isCreated;
    }

    public async Task<Customer?> GetAsync(Guid id)
    {
        var customerDto = await _customerRepository.GetAsync(id);
        return customerDto?.ToCustomer();
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        var customerDtos = await _customerRepository.GetAllAsync();
        return customerDtos.Select(x => x.ToCustomer());
    }

    public async Task<bool> UpdateAsync(Customer customer)
    {
        var customerDto = customer.ToCustomerDto();
        
        var isValidGitHubUser = await _gitHubService.IsValidGitHubUser(customer.GitHubUsername);
        if (!isValidGitHubUser)
        {
            var message = $"There is no GitHub user with username {customer.GitHubUsername}";
            throw new ValidationException(message, GenerateValidationError(nameof(customer.GitHubUsername), message));
        }
        
        var isUpdated = await _customerRepository.UpdateAsync(customerDto);
        if(isUpdated)
        {
            // SNS messenger
            await _snsMessenger.PublishMessageAsync(customer.ToCustomerUpdatedMessage());
        }

        return isUpdated;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var isDeleted = await _customerRepository.DeleteAsync(id);
        if(isDeleted)
        {
            // SNS messenger
            await _snsMessenger.PublishMessageAsync(new CustomerDeleted{
                Id = id,
            });
        }

        return isDeleted;
    }

    private static ValidationFailure[] GenerateValidationError(string paramName, string message)
    {
        return new []
        {
            new ValidationFailure(paramName, message)
        };
    }
}
