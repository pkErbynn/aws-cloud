Publisher is an SNS:
- Because it needs that to publish to a topic

Consumer is still SQS:
- Because consuming servie can be Queue
- Multiple consuming SQS services
    - SqsConsumerClient from SQS module
    - SqsConsumerClient2 in this module
    - Each queue has representation in AWS Queues cloud, 
        - ie, "customers"
        - and "customers2"


Running CLI consumers
- Running new and old consumers and
- Running publisher, consumers recieve messages in their terminals

Running real services
- Integrate Sns library in to the "Customers.Api" 
- Then consumed by the "Customers.Consumer" service

