using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Core.Validation;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.IssuesReviews.Entities;
using SachkovTech.Issues.Domain.IssuesReviews.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Commands.AddComment;

public class AddCommentHandler : ICommandHandler<Guid, AddCommentCommand>
{
    private readonly IIssuesReviewRepository _issuesReviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<AddCommentCommand> _validator;
    private readonly ILogger<AddCommentHandler> _logger;

    public AddCommentHandler(
        IIssuesReviewRepository issuesReviewRepository,
        IUnitOfWork unitOfWork,
        IValidator<AddCommentCommand> validator,
        ILogger<AddCommentHandler> logger)
    {
        _issuesReviewRepository = issuesReviewRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        AddCommentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        var issueReviewResult = await _issuesReviewRepository
            .GetIssueReview(command.UserId, command.IssueId, cancellationToken);

        if (issueReviewResult.IsFailure)
            return issueReviewResult.Error.ToErrorList();

        var message = Message.Create(command.Message).Value;

        var comment = new Comment(CommentId.NewCommentId(), command.IssueId, message);

        var addCommentResult = issueReviewResult.Value.AddComment(comment);

        if (addCommentResult.IsFailure)
            return addCommentResult.Error.ToErrorList();

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation(
            "Comment {commentId} was created in issueReview {issueReviewId}",
            comment.Id.Value,
            issueReviewResult.Value.Id.Value);

        return comment.Id.Value;
    }
}