using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Core.Abstractions;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Contracts.ModuleComplition;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Queries.GetUserModuleById;

public class GetUserModuleByIdHandler : IQueryHandlerWithResult<UserModuleDto, GetUserModuleByIdQuery>
{
    private readonly IIssuesReadDbContext _readDbContext;

    public GetUserModuleByIdHandler(IIssuesReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<UserModuleDto, ErrorList>> Handle(
        GetUserModuleByIdQuery query,
        CancellationToken cancellationToken)
    {
        var module = await _readDbContext.ReadModules
            .FirstOrDefaultAsync(x => x.Id == query.ModuleId, cancellationToken: cancellationToken);

        if (module is null)
            return Errors.General.NotFound().ToErrorList();

        var userModule = await _readDbContext.ReadUserModules
            .FirstOrDefaultAsync(x =>
                x.ModuleId == query.ModuleId &&
                x.UserId == query.UserId, 
                cancellationToken);
        
        var atWork = userModule?.AtWork ?? false;

        return new UserModuleDto(module.Id, module.Title.Value, module.Description.Value, atWork);
    }
}