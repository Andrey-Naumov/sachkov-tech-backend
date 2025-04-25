using Microsoft.AspNetCore.Mvc;
using SachkovTech.Framework;
using SachkovTech.Framework.Authorization;
using SachkovTech.Issues.Application.Features.LessonsComplition.Command.CompleteView;
using SachkovTech.Issues.Application.Features.LessonsComplition.Command.UncompleteViewed;
using SachkovTech.Issues.Contracts.LessonsComplition;

namespace SachkovTech.Issues.Presentation.LessonsComplition;

public class LessonsComplitionController : ApplicationController
{
    [HttpPost("completed-view")]
    public async Task<ActionResult> CompletedView(
        [FromBody] CompletedViewRequest request,
        [FromServices] CompleteViewHandler handler,
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
        [FromServices] UncompleteViewedLessonHandler handler,
        [FromServices] UserScopedData userScopedData,
        CancellationToken cancellationToken = default)
    {
        var command = new UncompleteViewedLessonCommand(userScopedData.UserId, request.LessonId);

        var result = await handler.Handle(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok();
    }
}