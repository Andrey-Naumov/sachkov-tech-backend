using MassTransit;

namespace FaqService.Consumers.Definitions;

public class DeleteQuestionIndexConsumerDefinition : ConsumerDefinition<DeleteQuestionIndexConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<DeleteQuestionIndexConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(
            c => c.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)));
    }
}
