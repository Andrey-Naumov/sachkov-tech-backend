using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.Module;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Interfaces;

public interface IModulesRepository
{
    Task<Guid> Add(Module issue, CancellationToken cancellationToken = default);

    Task<Result<Module, Error>> GetById(ModuleId moduleId, CancellationToken cancellationToken = default);

    Task<Result<Module, Error>> GetByTitle(Title title, CancellationToken cancellationToken = default);
}