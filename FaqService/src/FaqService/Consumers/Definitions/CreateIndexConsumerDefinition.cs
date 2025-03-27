using MassTransit;

namespace FaqService.Consumers.Definitions;

public class CreateIndexConsumerDefinition : ConsumerDefinition<IndexQuestionConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<IndexQuestionConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(
            c => c.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)));
    }
}
