using FaqService.Contracts.Messaging.Commands;
using FaqService.Contracts.Requests;
using FaqService.Entities.Elastic;
using Nest;

namespace FaqService.Infrastructure.Repositories;

public class SearchRepository
{
    private readonly IElasticClient _client;

    public SearchRepository(IElasticClient client)
    {
        _client = client;
    }

    public async Task<List<Guid>> SearchQuestions(GetQuestionsQuery query, CancellationToken cancellationToken = default)
    {
        var searchResponse = await ExecuteSearchQuery(query, cancellationToken);

        var ids = searchResponse.Hits
            .Select(hit => Guid.TryParse(hit.Id, out Guid guid) ? guid : Guid.Empty)
            .Where(guid => guid != Guid.Empty)
            .ToList();
        return ids;
    }

    public async Task<bool> IndexQuestion(IndexQuestionCommand command)
    {
        var questionElastic = new QuestionElastic
        {
            Id = command.Id,
            Title = command.Title,
            Description = command.Description,
            PullRequestLink = command.PullRequestLink,
            Status = command.Status.ToString(),
            CreatedAt = command.CreatedAt,
            Tags = command.Tags ?? [],
            IssueId = command.IssueId,
            LessonId = command.LessonId,
        };

        var response = await _client.IndexDocumentAsync(questionElastic);
        if (response.IsValid) return true;
        else return false;
    }

    public async Task<bool> DeleteQuestion(Guid questionId, CancellationToken cancellationToken = default)
    {
        var response = await _client.DeleteAsync<QuestionElastic>(questionId, d => d
                .Index("questions"),
            cancellationToken);

        return response.IsValid;
    }

    private async Task<ISearchResponse<QuestionElastic>> ExecuteSearchQuery(
        GetQuestionsQuery query, CancellationToken cancellationToken = default)
    {
        return await _client.SearchAsync<QuestionElastic>(
            s => s.Query(q => q
                .Bool(b => b
                    .Must(
                        m => m.MultiMatch(mm => mm
                            .Fields(f => f
                                .Field(p => p.Title, boost: 1.0)
                                .Field(p => p.Description, boost: 2.0))
                            .Query(query.SearchText ?? string.Empty)
                            .Fuzziness(Fuzziness.Auto)),
                        m => m.Term(t =>
                            t.Field(p => p.Status.Suffix("keyword"))
                                .Value(query.Status?.ToString())),
                        m => query.Tags is not null && query.Tags.Any()
                            ? m.Terms(t => t.Field(p => p.Tags.Suffix("keyword")).Terms(query.Tags))
                            : null,
                        m => query.IssueId.HasValue
                            ? m.Term(t => t.Field(p => p.IssueId.Suffix("keyword")).Value(query.IssueId.ToString()))
                            : null,
                        m => query.LessonId.HasValue
                            ? m.Term(t =>
                                t.Field(p => p.LessonId.Suffix("keyword")).Value(query.LessonId.ToString()))
                            : null))).Size(100),
            cancellationToken);
    }
}