## Producers of Kafka Integration Move Dynamically

DynamicKafkaIntegration is the is a _free, open-source_ library to making dynamic Kafka producer in MassTransit provider completely generic for all
developers' convenience and introducing their Producers and TopicEndPoints immediately.

---

## Tech stack And Used Library:

C#  
.NET 6.0  
Humanizer.Core 2.14.1  
MassTransit 8.0.6  
MassTransit.Kafka 8.0.6  
Microsoft.Extensions.DependencyInjection.Abstractions 6.0.0

---

Dynamic.KafkaIntegration.Producer is now available in NuGet.  
NuGet library:

Install-Package Dynamic.KafkaIntegration.Producer -Version 0.1.21

## Guide

### STEP1:

after install package from NuGet you can inject provider in MassTransit provider:

```csharp
services.AddMassTransit(x =>  
{  
...  
x.AddRider(rider =>  
{  
 rider.AddProducers(assemblies);  
 ...
   rider.UsingKafka((context, k) =>
        {
             ...
         });
      });
   });
}
```

### STEP2:

you must inherit your producer by IKafkaProducer:

```csharp
public class KafkaMessage:IKafkaProducer
{
    public string Text { get; set; }
}
```
### **if you need TopicEndPoint and any configuration for it and Producer you must read STEP3:** 
### STEP3

Before this library, if you wanted to introduce producer configuration in AddMassTransit, you should work as following:

```csharp
             services.AddMassTransit(busConfig =>
            {
                busConfig.AddRider(riderConfig =>
                {
                    ...


                    riderConfig.AddProducer<string,KafkaMessage>(nameof(KafkaMessage.UnderScore()), (riderContext,producerConfig) =>
                        {
                            var serializerConfig = new AvroSerializerConfig
                            {
                                SubjectNameStrategy = SubjectNameStrategy.Record,
                                AutoRegisterSchemas = true
                            };

                            var serializer = new MultipleTypeSerializer<ITaskEvent>(multipleTypeConfig, schemaRegistryClient, serializerConfig);
                            producerConfig.SetKeySerializer(new AvroSerializer<string>(schemaRegistryClient).AsSyncOverAsync());
                            producerConfig.SetValueSerializer(serializer.AsSyncOverAsync());
                        });

                    riderConfig.AddConsumersFromNamespaceContaining<TaskRequestedConsumer>();

                    riderConfig.UsingKafka((riderContext,kafkaConfig) =>
                    {
                        ...
                    });
                });
            });
```

now you can inherit from IKafkaProducerConfiguration and do your implementation:

```csharp
public class KafkaMessageConfiguration:IKafkaProducerConfiguration<KafkaMessage>
{
    public void Configure(IRiderRegistrationContext context, IKafkaProducerConfigurator<Null, KafkaMessage> configurator)
    {
         var serializerConfig = new AvroSerializerConfig
         {
             SubjectNameStrategy = SubjectNameStrategy.Record,
             AutoRegisterSchemas = true
         };

         var serializer = new MultipleTypeSerializer<ITaskEvent>(multipleTypeConfig, schemaRegistryClient, serializerConfig);
         producerConfig.SetKeySerializer(new AvroSerializer<string>(schemaRegistryClient).AsSyncOverAsync());
         producerConfig.SetValueSerializer(serializer.AsSyncOverAsync());
    }

    public ProducerConfig ProducerConfig { get; set; }
}
```
and if you wanted to use TopicEndPoint previously you have to introduce them like bellow
```csharp
builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));

    x.AddRider(rider =>
    {
        ...
        rider.UsingKafka((context, k) =>
        {
            ...
        
            k.TopicEndpoint<KafkaMessage>(nameof(KafkaMessage).Underscore(), "App2", e =>
            {
                e.ConfigureConsumer<KafkaMessageConsumer>(context);
                e.AutoOffsetReset = AutoOffsetReset.Earliest;
                e.CreateIfMissing(t =>
                {
                    t.NumPartitions = 1; //number of partitions
                    t.ReplicationFactor = 1; //number of replicas
                });
            });
        });
    });
});
```

now you can add TopicEndPoint Dynamically into AddMassTransit provider

```csharp
services.AddMassTransit(x =>  
{  
...  
x.AddRider(rider =>  
{  
 ...
   rider.UsingKafka((context, k) =>
        {
             ...
             k.AddTopicEndPoints(context, assemblies);
         });
      });
   });
}
```
and if you have any configuration for TopicEndPoint you can inherit from `TopicEndPoint<>` class and use producer in generic type:
```csharp
public class MarketingQueueMessageEndTopicPoint:TopicEndPoint<KafkaMessage>
{
    private readonly IRiderRegistrationContext _context;
    public MarketingQueueMessageEndTopicPoint(IRiderRegistrationContext context) : base(context)
    {
        _context = context;
    }

    public override string GroupId => "App2";
    protected override void ActionMethod(IKafkaTopicReceiveEndpointConfigurator<Ignore, MarketingQueueMessage> configurator)
    {
        configurator.ConfigureConsumer<KafkaMessageConsumer>(_context);
        configurator.AutoOffsetReset = AutoOffsetReset.Earliest;
        configurator.CreateIfMissing(t =>
        {
            t.NumPartitions = 1; //number of partitions
            t.ReplicationFactor = 1; //number of replicas
        });
    }
}
```
and all of your configuration should register by reflective libraries and highly performance.

if you need sample you can see this project:
https://github.com/mortezarezaei68/.NET-micro-service