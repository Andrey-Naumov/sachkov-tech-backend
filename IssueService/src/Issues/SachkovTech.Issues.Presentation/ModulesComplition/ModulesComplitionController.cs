using Microsoft.AspNetCore.Mvc;
using SachkovTech.Framework;
using SachkovTech.Issues.Application.Features.ModulesComplition.Command.StartModuleExecution;
using SachkovTech.Issues.Contracts.ModuleProgress;

namespace SachkovTech.Issues.Presentation.ModulesComplition;

public class ModulesComplitionController : ApplicationController
{
    [HttpPost]
    public async Task<IActionResult> StartModuleExecution(
        [FromBody] StartModuleExecutionRequest request,
        [FromServices] StartModuleExecutionHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new StartModuleExecutionCommand(request.UserId, request.ModuleId);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }
}