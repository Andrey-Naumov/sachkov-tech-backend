namespace SachkovTech.Issues.Presentation.Hubs;

public interface ILessonVideoProcessingClient
{
    Task ProgressUpdate(double progress, CancellationToken cancellationToken = default);

    Task LessonVideoProcessed(Guid lessonId, CancellationToken contextCancellationToken);
}