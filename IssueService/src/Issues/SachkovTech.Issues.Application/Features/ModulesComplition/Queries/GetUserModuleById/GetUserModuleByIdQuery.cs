using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Queries.GetUserModuleById;

public record GetUserModuleByIdQuery(Guid ModuleId, Guid UserId) : IQuery;