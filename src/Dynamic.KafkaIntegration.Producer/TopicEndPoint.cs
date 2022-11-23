using Confluent.Kafka;
using MassTransit;

namespace Dynamic.KafkaIntegration.Producer;

public abstract class TopicEndPoint
    <TProducer> where TProducer : class, IKafkaProducer, new()
{
    protected TopicEndPoint(IRiderRegistrationContext context)
    {
    }
    public abstract string GroupId { get; }

    public string? TopicName => typeof(TProducer).Name;

    protected abstract void ActionMethod(IKafkaTopicReceiveEndpointConfigurator<Ignore, TProducer> configurator);
}