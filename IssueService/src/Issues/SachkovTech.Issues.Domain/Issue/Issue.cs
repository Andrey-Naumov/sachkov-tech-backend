using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.Issue.Events;
using SachkovTech.Issues.Domain.Issue.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.Issue;

public class Issue : DomainEntity<IssueId>, ISoftDeletable
{
    private List<FileId> _files = [];

    // ef core
    private Issue(IssueId id)
        : base(id)
    {
    }

    public Issue(
        IssueId id,
        Title title,
        Description description,
        LessonId? lessonId,
        ModuleId moduleId,
        Experience experience,
        IEnumerable<Guid> tags,
        IEnumerable<FileId> files)
        : base(id)
    {
        Title = title;
        Description = description;
        LessonId = lessonId;
        ModuleId = moduleId;
        Experience = experience;
        _files = files.ToList();

        UpdateTags(tags);
        AddDomainEvent(new IssueCreatedDomainEvent(id, moduleId));
    }

    public Experience Experience { get; private set; } = default!;

    public Title Title { get; private set; } = default!;

    public Description Description { get; private set; } = default!;

    public LessonId? LessonId { get; private set; }

    public ModuleId ModuleId { get; private set; } = null!;

    public Guid[] Tags { get; private set; }

    public IReadOnlyList<FileId> Files => _files;

    public bool IsDeleted { get; private set; }

    public DateTime? DeletionDate { get; private set; }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletionDate = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletionDate = null;
    }

    public void UpdateFiles(IEnumerable<FileId> files)
    {
        _files = files.ToList();
    }

    public UnitResult<Error> UpdateMainInfo(
        Title title,
        Description description,
        LessonId? lessonId,
        ModuleId moduleId,
        Experience experience,
        IEnumerable<Guid> tags)
    {
        Title = title;
        Description = description;
        LessonId = lessonId;
        ModuleId = moduleId;
        Experience = experience;

        UpdateTags(tags);

        return Result.Success<Error>();
    }

    private void UpdateTags(IEnumerable<Guid> tags)
    {
        var updatedTags = tags.ToArray();

        // var assignedTags = GetAssignedTags(updatedTags);
        // if (assignedTags.Any())
        //     AddDomainEvent(new TagsAssignedDomainEvent(Id, assignedTags));
        //
        // var unassignedTags = GetUnassignedTags(updatedTags);
        // if (unassignedTags.Any())
        //     AddDomainEvent(new TagsUnassignedDomainEvent(Id, unassignedTags));

        Tags = updatedTags;
    }

    private Guid[] GetAssignedTags(IEnumerable<Guid> newTags) 
        => newTags.Except(Tags).ToArray();

    private Guid[] GetUnassignedTags(IEnumerable<Guid> newTags)
        => Tags.Except(newTags).ToArray();
}