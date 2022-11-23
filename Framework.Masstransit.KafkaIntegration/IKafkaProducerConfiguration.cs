using Confluent.Kafka;
using MassTransit;

namespace Framework.Masstransit.KafkaIntegration
{
    /// <summary>
    /// Set configuration and ProducerConfig
    /// </summary>
    /// <typeparam name="TProducer">Type is inherit from IKafkaProducer</typeparam>
    public interface IKafkaProducerConfiguration<TProducer> where TProducer : class, IKafkaProducer
    {
        /// <summary>
        /// Set configuration for kafka producer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configurator"></param>
        public void Configure(IRiderRegistrationContext context,
            IKafkaProducerConfigurator<Null, TProducer> configurator);

        /// <summary>
        /// Producer config
        /// </summary>
        public ProducerConfig ProducerConfig { get; set; }
    }
}