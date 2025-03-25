namespace TagService.API.Contracts.Requests;

public record AddTagRequest(string Name, string Description, DateTime CreatedAt);