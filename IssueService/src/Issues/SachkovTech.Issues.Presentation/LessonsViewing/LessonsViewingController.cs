using Microsoft.AspNetCore.Mvc;
using SachkovTech.Framework;
using SachkovTech.Framework.Authorization;
using SachkovTech.Issues.Application.Features.LessonsViewing.Command.CompletedView;
using SachkovTech.Issues.Application.Features.LessonsViewing.Command.UncompletedViewed;
using SachkovTech.Issues.Contracts.LessonsViewing;

namespace SachkovTech.Issues.Presentation.LessonsViewing;

public class LessonsViewingController : ApplicationController
{
    [HttpPost("completed-view")]
    public async Task<ActionResult> CompletedView(
        [FromBody] CompletedViewRequest request,
        [FromServices] CompletedViewHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken = default)
    {
        var command = new CompletedViewCommand(userScopedData.UserId, request.LessonId);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }

    [HttpPut("uncompleted-view")]
    public async Task<ActionResult> UncompletedView(
        [FromBody] UncompletedViewedLessonRequest request,
        [FromServices] UncompletedViewedLessonHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken = default)
    {
        var command = new UncompletedViewedLessonCommand(userScopedData.UserId, request.LessonId);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok();
    }
}