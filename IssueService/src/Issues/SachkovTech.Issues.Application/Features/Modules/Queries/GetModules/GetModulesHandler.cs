using Microsoft.EntityFrameworkCore;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Contracts.Module;

namespace SachkovTech.Issues.Application.Features.Modules.Queries.GetModules;

public class GetModulesHandler : IQueryHandler<PagedList<ModuleDto>, GetModulesQuery>
{
    private readonly IIssuesReadDbContext _readDbContext;

    public GetModulesHandler(IIssuesReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<PagedList<ModuleDto>> Handle(
        GetModulesQuery query,
        CancellationToken cancellationToken = default)
    {
        var modulesQuery = _readDbContext.ReadModules;

        if (!string.IsNullOrWhiteSpace(query.Title))
        {
            modulesQuery = modulesQuery
                .Where(m => EF.Functions
                    .Like(m.Title.Value.ToLower(), $"%{query.Title.ToLower()}%"));
        }

        var modulesPagedList = await modulesQuery.ToPagedList(
            query.Page,
            query.PageSize,
            m => new ModuleDto(
                m.Id,
                m.Title.Value,
                m.Description.Value),
            cancellationToken);

        return modulesPagedList;
    }
}