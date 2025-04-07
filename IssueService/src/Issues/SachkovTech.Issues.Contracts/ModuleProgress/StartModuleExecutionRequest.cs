namespace SachkovTech.Issues.Contracts.ModuleProgress;

public record StartModuleExecutionRequest(Guid ModuleId, Guid UserId);