using FluentAssertions;
using SachkovTech.Issues.Domain.IssuesReviews;
using SachkovTech.Issues.Domain.IssuesReviews.Entities;
using SachkovTech.Issues.Domain.IssuesReviews.Enums;
using SachkovTech.Issues.Domain.IssuesReviews.Events;
using SachkovTech.Issues.Domain.IssuesReviews.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.IssueReviews.UnitTests.Domain;

public class IssuesReviewTests
{
    [Fact]
    public void Start_review_by_reviewer()
    {
        // Arrange
        var validReviewerId = UserId.NewUserId();
        var issueReview = CreateAndFillIssueReview();

        // Act
        issueReview.StartReview(validReviewerId);

        // Assert
        issueReview.ReviewerId.Should().Be(validReviewerId);
        issueReview.IssueReviewStatus.Should().Be(IssueReviewStatus.OnReview);
        issueReview.ReviewerId.Should().NotBeNull();
    }

    [Fact]
    public void Send_issue_for_revision_by_reviewer()
    {
        // Arrange
        var reviewerId = UserId.NewUserId();
        var issueReview = CreateAndFillIssueReview();
        issueReview.StartReview(reviewerId);

        // Act
        var result = issueReview.SendIssueForRevision(reviewerId);

        // Assert
        var domainEvent = issueReview.DomainEvents.SingleOrDefault() as IssueSentForRevisionDomainEvent;

        result.IsSuccess.Should().BeTrue();
        domainEvent.Should().NotBeNull();
        domainEvent!.IssueId.Should().Be(issueReview.IssueId);
        issueReview.IssueReviewStatus.Should().Be(IssueReviewStatus.AskedForRevision);
    }

    [Fact]
    public void Send_issue_for_revision_invalid_reviewer_id()
    {
        // Arrange
        var validReviewerId = UserId.NewUserId();
        var invalidReviewerId = UserId.NewUserId();
        var issueReview = CreateAndFillIssueReview();
        issueReview.StartReview(validReviewerId);

        // Act
        var result = issueReview.SendIssueForRevision(invalidReviewerId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Errors.Auth.InvalidCredentials());
    }

    [Fact]
    public void Approve_with_valid_reviewer_id()
    {
        // Arrange
        var reviewerId = UserId.NewUserId();

        var issueReview = CreateAndFillIssueReview();
        issueReview.StartReview(reviewerId);

        // Act
        var result = issueReview.Approve(reviewerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        issueReview.IssueReviewStatus.Should().Be(IssueReviewStatus.Accepted);
    }

    [Fact]
    public void Approve_with_invalid_reviewer_id()
    {
        // Arrange
        var validReviewerId = UserId.NewUserId();
        var invalidReviewerId = UserId.NewUserId();

        var issueReview = CreateAndFillIssueReview();
        issueReview.StartReview(validReviewerId);

        // Act
        var result = issueReview.Approve(invalidReviewerId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Errors.Auth.InvalidCredentials());
    }

    [Fact]
    public void Add_comment_from_author_or_reviewer()
    {
        // Arrange
        var authorId = UserId.NewUserId();
        var reviewerId = UserId.NewUserId();

        var issueReview = new IssueReview(
            IssueReviewId.NewIssueReviewId(),
            IssueId.NewIssueId(),
            authorId,
            PullRequestUrl.Empty);

        issueReview.StartReview(reviewerId);

        var commentFromAuthor = new Comment(CommentId.NewCommentId(), authorId, Message.Create("Test1").Value);
        var commentFromReviewer = new Comment(CommentId.NewCommentId(), reviewerId, Message.Create("Test2").Value);

        // Act
        var resultFromReviewer = issueReview.AddComment(commentFromReviewer);
        var resultFromAuthor = issueReview.AddComment(commentFromAuthor);

        // Assert
        resultFromReviewer.IsSuccess.Should().BeTrue();
        resultFromAuthor.IsSuccess.Should().BeTrue();

        issueReview.Comments.Should().Contain(commentFromAuthor);
        issueReview.Comments.Should().Contain(commentFromReviewer);
    }

    [Fact]
    public void Add_comment_invalid_user()
    {
        // Arrange
        var reviewerId = UserId.NewUserId();
        var invalidUserId = UserId.NewUserId();
        var issueReview = CreateAndFillIssueReview();
        issueReview.StartReview(reviewerId);

        var invalidComment = new Comment(CommentId.NewCommentId(), invalidUserId, Message.Create("Test").Value);

        // Act
        var result = issueReview.AddComment(invalidComment);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Errors.General.ValueIsInvalid("userId"));
        issueReview.Comments.Should().NotContain(invalidComment);
    }

    [Fact]
    public void Delete_comment_by_author()
    {
        // Arrange
        var userId = UserId.NewUserId();
        var comment = new Comment(CommentId.NewCommentId(), userId, Message.Create("Test1").Value);
        var issueReview = CreateAndFillIssueReview();
        issueReview.AddComment(comment);

        // Act
        var result = issueReview.DeleteComment(comment.Id, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        issueReview.Comments.Should().NotContain(comment);
    }

    [Fact]
    public void Delete_a_non_author_or_reviewer_comment()
    {
        // Arrange
        var authorId = UserId.NewUserId();
        var reviewerId = UserId.NewUserId();
        var otherUserId = UserId.NewUserId();
        var comment = new Comment(CommentId.NewCommentId(), authorId, Message.Create("Test1").Value);

        var issueReview = CreateAndFillIssueReview();
        issueReview.AddComment(comment);
        issueReview.StartReview(reviewerId);

        // Act
        var result = issueReview.DeleteComment(comment.Id, otherUserId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Errors.General.ValueIsInvalid("userId"));
    }

    private IssueReview CreateAndFillIssueReview()
    {
        return new IssueReview(
            IssueReviewId.NewIssueReviewId(),
            IssueId.NewIssueId(),
            UserId.NewUserId(),
            PullRequestUrl.Empty);
    }
}