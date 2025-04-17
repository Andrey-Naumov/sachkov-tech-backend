using IssueService.Communication.Lesson;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Issues.Contracts.Lesson.IntegrationEvents;
using SharedKernel;
using SharedKernel.Exeptions;
using TagService.Infrastructure;

namespace TagService.Consumers;

public class LessonCreatedConsumer : IConsumer<LessonCreatedIntegrationEvent>
{
    private readonly ILessonService _lessonService;
    private readonly ApplicationDbContext _dbContext;

    public LessonCreatedConsumer(ILessonService lessonService, ApplicationDbContext dbContext)
    {
        _lessonService = lessonService;
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<LessonCreatedIntegrationEvent> context)
    {
        var uploadedEvent = context.Message;

        var lessonResult = await _lessonService.GetLessonById(uploadedEvent.LessonId, context.CancellationToken);
        if (lessonResult.IsFailure)
            // TODO: для ErrorList'a сделать конструктор в Exception'e, а потом тут поменять на LessonResult.Error
            throw new NotFoundException(Errors.General.NotFound());

        var tags = await _dbContext.Tags
            .Where(t => lessonResult.Value.Tags.Contains(t.Id))
            .ToListAsync(context.CancellationToken);

        foreach (var tag in tags)
        {
            tag.UsagesIncrease();
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}