using FaqService.Contracts.Messaging.Events;
using FaqService.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FaqService.Extensions;

public class ElasticIndexRecoveryService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ElasticIndexRecoveryService> _logger;

    public ElasticIndexRecoveryService(
        ApplicationDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        ILogger<ElasticIndexRecoveryService> logger)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task RestoreElasticIndex(Guid questionId, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Restoring previous index state for question {questionId}", questionId);

        var question = await _dbContext.Questions.SingleOrDefaultAsync(p => p.Id == questionId, cancellationToken);

        if (question is null)
        {
            _logger.LogError("Failed to restore index for question {questionId}. Question not found after rollback.", questionId);
            return;
        }

        try
        {
            QuestionUpdatedEvent questionUpdatedEvent = new(
                question.Id,
                question.Title,
                question.Description,
                question.PullRequestLink.Value,
                question.Status,
                question.CreatedAt,
                question.Tags,
                question.IssueId,
                question.LessonId);

            await _publishEndpoint.Publish(questionUpdatedEvent, cancellationToken);
            _logger.LogInformation("Successfully published index restoration command for question {questionId}.", questionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish index restoration command for question {questiontId} after rollback.", questionId);
        }
    }
}