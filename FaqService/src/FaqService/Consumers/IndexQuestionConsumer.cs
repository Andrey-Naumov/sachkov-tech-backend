using FaqService.Contracts.Messaging.Commands;
using FaqService.Contracts.Messaging.Events;
using FaqService.Infrastructure.Repositories;
using MassTransit;

namespace FaqService.Consumers;

/// <summary>
/// MassTransit+RabbitMQ consumer.
/// </summary>
public sealed class IndexQuestionConsumer(SearchRepository searchRepository)
    : IConsumer<QuestionUpdatedEvent>
{
    private readonly SearchRepository _searchRepository = searchRepository;

    /// <summary>
    /// MassTransit+RabbitMQ consumer for ElasticSearch index creation or update.
    /// </summary>
    /// <param name="context">Consume context.</param>
    /// <returns>Task.</returns>
    /// <throws>ApplicationException.</throws>
    public async Task Consume(ConsumeContext<QuestionUpdatedEvent> context)
    {
        var indexQuestionCommand = new IndexQuestionCommand(
            context.Message.Id,
            context.Message.Title,
            context.Message.Description,
            context.Message.PullRequestLink,
            context.Message.Status,
            context.Message.CreatedAt,
            context.Message.Tags,
            context.Message.IssueId,
            context.Message.LessonId);

        var success = await _searchRepository.IndexQuestion(indexQuestionCommand);
        if (success == false)
            throw new ApplicationException($"Error occured while indexing question with id {context.Message.Id}");
    }
}
