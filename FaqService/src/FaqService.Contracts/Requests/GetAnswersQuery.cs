namespace FaqService.Contracts.Requests;

public record GetAnswersQuery(Guid? Cursor, int Limit = 10);