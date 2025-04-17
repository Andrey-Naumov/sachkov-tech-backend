using MassTransit;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Issues.Application.Features.Lessons.Command.DeleteTagFromLesson;
using SachkovTech.Issues.Infrastructure.DbContexts;
using TagService.Contracts.IntegrationEvents;

namespace SachkovTech.Issues.Infrastructure.Consumers;

public class TagDeletedConsumer : IConsumer<TagDeletedIntegrationEvent>
{
    private readonly IssuesDbContext _dbContext;
    private readonly ICommandHandler<DeleteTagFromLessonCommand> _handler;
    private readonly ILogger<LessonVideoProcessedConsumer> _logger;

    public TagDeletedConsumer(
        IssuesDbContext dbContext,
        ICommandHandler<DeleteTagFromLessonCommand> handler,
        ILogger<LessonVideoProcessedConsumer> logger)
    {
        _dbContext = dbContext;
        _handler = handler;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TagDeletedIntegrationEvent> context)
    {
        var tagDeletedEvent = context.Message;

        var command = new DeleteTagFromLessonCommand(tagDeletedEvent.TagId);

        await _handler.Handle(command, context.CancellationToken);

        _logger.LogInformation("Tag deleted: {TagId}", tagDeletedEvent.TagId);

        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}