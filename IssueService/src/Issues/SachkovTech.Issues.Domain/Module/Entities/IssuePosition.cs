using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.Module.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.Issues.Domain.Module.Entities;

public class IssuePosition : Entity<IssuePositionId>, IPositionable
{
    public IssuePosition(IssuePositionId id, IssueId issueId, Position position)
        : base(id)
    {
        IssueId = issueId;
        Position = position;
    }

    // ef core
    private IssuePosition()
    {
    }

    public IssueId IssueId { get; private set; } = null!;

    public Position Position { get; private set; } = null!;

    public void SetPosition(Position newPosition)
    {
        Position = newPosition;
    }
}