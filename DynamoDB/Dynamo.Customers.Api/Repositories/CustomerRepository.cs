using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Dynamo.Customers.Api.Contracts.Data;

namespace Dynamo.Customers.Api.Repositories;


/// <summary>
/// This Repository doesn't connect with SqliteDb 
/// ...but rather AWS DynamoDB in cloud
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName = "customers";   // should come from config

    public CustomerRepository(IAmazonDynamoDB dynamoDb)
    {
        _dynamoDb = dynamoDb;
    }

    public async Task<bool> CreateAsync(CustomerDto customer)
    {
        customer.UpdateAt = DateTime.UtcNow;
        var customerAsJson = JsonSerializer.Serialize(customer);

        /// An attribute map defines the attributes(name, email, etc) and their values(john, j@erbynn) for an item in order to be stored within a DynamoDB table. A prepared data ready to be pushed/written to db
        var customerAsAttribute = Document.FromJson(customerAsJson).ToAttributeMap();
        
        var createItemRequest = new PutItemRequest
        {
            TableName = _tableName,
            Item = customerAsAttribute,
            ConditionExpression = "attribute_not_exists(pk) and attribute_not_exists(sk)"   // to ensure newly resource creating and reliabiliy, preventing data overriding
        };

        var response = await _dynamoDb.PutItemAsync(createItemRequest);

        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<CustomerDto?> GetAsync(Guid id)
    {
        var getItemRequest = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "pk", new AttributeValue { S = id.ToString() } },
                { "sk", new AttributeValue { S = id.ToString() } },
            }
        };

        var response = await _dynamoDb.GetItemAsync(getItemRequest);
        if (response.Item.Count == 0)
        {
            return null;
        }

        var itemAsDocument = Document.FromAttributeMap(response.Item);
        var itemAsObject = JsonSerializer.Deserialize<CustomerDto>(itemAsDocument.ToJson());
        return itemAsObject;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync()
    {
        var scanRequest = new ScanRequest
        {
            TableName = _tableName
        };
        var response = await _dynamoDb.ScanAsync(scanRequest);
        return response.Items.Select(x => 
        {
            var jsonItem = Document.FromAttributeMap(x).ToJson();
            return JsonSerializer.Deserialize<CustomerDto>(jsonItem);
        })!;
    }

    public async Task<bool> UpdateAsync(CustomerDto customer, DateTime requestStarted)
    {
        customer.UpdateAt = DateTime.UtcNow;
        var customerAsJson = JsonSerializer.Serialize(customer);
        var customerAsAttribute = Document.FromJson(customerAsJson).ToAttributeMap();
        
        var updateItemRequest = new PutItemRequest
        {
            TableName = _tableName,
            Item = customerAsAttribute,

            // incomming request(@ Controller-level) time should be greater that updated time on server(@ Repo-level). 
            // Condition must be satisfied before update operation happens successfylly. Stop if otherwise
            ConditionExpression = "UpdateAt < :requestStarted",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                /// The "O" represents a round-trip date/time pattern, which is a date/time format that preserves all the necessary information to accurately reconstruct the original date 
                /// and time when the string is parsed back into a date/time object. This means that the format includes not only the date and time but also the time zone information and fractional seconds, 
                /// ensuring that no data is lost during the conversion process.
                /// If not, might be based on system culture settings, missing some parts of the datetime
                {":requestStarted", new AttributeValue{S = requestStarted.ToString("O")}}
            }
        };

        var response = await _dynamoDb.PutItemAsync(updateItemRequest);

        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var deleteItemRequest = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "pk", new AttributeValue { S = id.ToString() } },
                { "sk", new AttributeValue { S = id.ToString() } },
            }
        };

        var response = await _dynamoDb.DeleteItemAsync(deleteItemRequest);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }
}
