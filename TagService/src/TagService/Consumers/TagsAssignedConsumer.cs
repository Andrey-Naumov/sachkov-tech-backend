using MassTransit;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Issues.Contracts.Tags;
using TagService.Infrastructure;

namespace TagService.Consumers;

public class TagsAssignedConsumer : IConsumer<TagsAssignedIntegrationEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public TagsAssignedConsumer(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<TagsAssignedIntegrationEvent> context)
    {
        var tagsAssignedEvent = context.Message;

        var tags = await _dbContext.Tags
            .Where(t => tagsAssignedEvent.TagIds.Contains(t.Id))
            .ToListAsync();
        
        foreach (var tag in tags)
        {
            tag.UsagesIncrease();
        }
        
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}