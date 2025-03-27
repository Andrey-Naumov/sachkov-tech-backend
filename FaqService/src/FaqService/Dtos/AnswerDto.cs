namespace FaqService.Dtos;

public record AnswerDto
{
    public Guid Id { get; init; }
    public bool IsSolution { get; init; }
    public Guid QuestionId { get; init; }
    public string Text { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public int Rating { get; init; }
    public DateTime CreatedAt { get; init; }
}