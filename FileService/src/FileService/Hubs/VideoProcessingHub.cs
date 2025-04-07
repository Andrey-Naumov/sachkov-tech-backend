using Microsoft.AspNetCore.SignalR;

namespace FileService.Hubs;

public interface IVideoProcessingClient
{
    Task ProgressUpdate(double progress, CancellationToken cancellationToken = default);
}

public class VideoProcessingHub : Hub<IVideoProcessingClient>
{
    public async Task JoinProcessingGroup(Guid processId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, processId.ToString());
    }
}