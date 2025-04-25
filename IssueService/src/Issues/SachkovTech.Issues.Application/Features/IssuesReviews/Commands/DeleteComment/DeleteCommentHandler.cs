using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Commands.DeleteComment;

public class DeleteCommentHandler : ICommandHandler<Guid, DeleteCommentCommand>
{
    private readonly IIssuesReviewRepository _issuesReviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCommentHandler> _logger;

    public DeleteCommentHandler(
        IIssuesReviewRepository issuesReviewRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteCommentHandler> logger)
    {
        _issuesReviewRepository = issuesReviewRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        DeleteCommentCommand command,
        CancellationToken cancellationToken = default)
    {
        var issueReviewResult = await _issuesReviewRepository
            .GetIssueReview(command.UserId, command.IssueId, cancellationToken);

        if (issueReviewResult.IsFailure)
            return issueReviewResult.Error.ToErrorList();

        var commentId = CommentId.Create(command.CommentId);

        var userId = UserId.Create(command.UserId);

        var addCommentResult = issueReviewResult.Value.DeleteComment(commentId, userId);

        if (addCommentResult.IsFailure)
            return addCommentResult.Error.ToErrorList();

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "Comment {commentId} was deleted in issueReview {issueReviewId}",
            command.CommentId,
            issueReviewResult.Value.Id.Value);

        return command.CommentId;
    }
}