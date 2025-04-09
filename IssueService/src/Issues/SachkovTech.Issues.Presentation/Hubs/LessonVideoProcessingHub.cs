using Microsoft.AspNetCore.SignalR;

namespace SachkovTech.Issues.Presentation.Hubs;

public class LessonVideoProcessingHub : Hub<ILessonVideoProcessingClient>
{
    public async Task JoinProcessingGroup(Guid lessonId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, lessonId.ToString());
    }
}