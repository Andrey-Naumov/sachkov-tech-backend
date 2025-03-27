using FaqService.Contracts.Messaging.Events;
using FaqService.Infrastructure.Repositories;
using MassTransit;

namespace FaqService.Consumers;

/// <summary>
/// MassTransit+RabbitMQ consumer.
/// </summary>
public sealed class DeleteQuestionIndexConsumer(SearchRepository searchRepository)
    : IConsumer<QuestionDeletedEvent>
{
    private readonly SearchRepository _searchRepository = searchRepository;

    /// <summary>
    /// MassTransit+RabbitMQ consumer for ElasticSearch index deletion.
    /// </summary>
    /// <param name="context">Consume context.</param>
    /// <returns>Task.</returns>
    /// /// <throws>ApplicationException.</throws>
    public async Task Consume(ConsumeContext<QuestionDeletedEvent> context)
    {
        var success = await _searchRepository.DeleteQuestion(context.Message.Id);
        if (success == false)
            throw new ApplicationException($"Error occured while deleting Question Index with id {context.Message.Id}");
    }
}
