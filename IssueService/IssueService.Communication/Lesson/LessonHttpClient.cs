using CSharpFunctionalExtensions;
using SachkovTech.Framework.Http;
using SachkovTech.Issues.Contracts.Lesson;
using SharedKernel;

namespace IssueService.Communication.Lesson;

public class LessonHttpClient(HttpClient httpClient) : ILessonService
{
    public async Task<Result<LessonDto, ErrorList>> GetLessonById(Guid lessonId, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"api/lessons/{lessonId}", cancellationToken);
        return await response.HandleResponseAsync<LessonDto>(cancellationToken);
    }
}