namespace FaqService.Contracts.Requests;

public record CreateAnswerRequest(string Text, Guid UserId);