using MassTransit;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Issues.Contracts.Tags;
using TagService.Infrastructure;

namespace TagService.Consumers;

public class TagsUnAssignedConsumer: IConsumer<TagsUnassignedIntegrationEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public TagsUnAssignedConsumer(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<TagsUnassignedIntegrationEvent> context)
    {
        var tagsUnassignedEvent = context.Message;

        var tags = await _dbContext.Tags
            .Where(t => tagsUnassignedEvent.TagIds.Contains(t.Id))
            .ToListAsync();
        
        foreach (var tag in tags)
        {
            tag.UsagesDecrease();
        }
        
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}