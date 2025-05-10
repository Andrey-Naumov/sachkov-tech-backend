using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.Issue.ValueObjects;
using SachkovTech.Issues.Domain.Lesson.Events;
using SachkovTech.Issues.Domain.Lesson.ValueObjects;
using SachkovTech.Issues.Domain.Tags.Events;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.Lesson;

public class Lesson : DomainEntity<LessonId>, ISoftDeletable
{
    // EF CORE
    private Lesson(LessonId id)
        : base(id)
    {
    }

    public Lesson(
        LessonId id,
        Guid moduleId,
        Title title,
        Description description,
        Experience experience,
        IEnumerable<Guid> tags,
        Guid[] issues)
        : base(id)
    {
        ModuleId = moduleId;
        Title = title;
        Description = description;
        Experience = experience;
        Issues = issues;

        UpdateTags(tags);
        AddDomainEvent(new LessonCreatedDomainEvent(id, moduleId));
    }

    public Guid ModuleId { get; private set; }

    public Title Title { get; private set; }

    public Description Description { get; private set; }

    public Experience Experience { get; private set; }

    public Guid[] Tags { get; private set; }

    public Guid[] Issues { get; private set; }

    public Preview AutoPreview { get; private set; } = Preview.None;

    public Video Video { get; private set; } = Video.None;

    public bool IsDeleted { get; private set; }

    public DateTime? DeletionDate { get; private set; }

    /// <summary>
    /// Метод, который полностью обновляет урок.
    /// </summary>
    /// <param name="title">Название.</param>
    /// <param name="description">Описание.</param>
    /// <param name="experience">Опыт за урок.</param>
    /// <param name="tags">Список тегов к уроку.</param>
    /// <param name="issues">Список задач к уроку.</param>
    public void Update(
        Title title,
        Description description,
        Experience experience,
        IEnumerable<Guid> tags,
        Guid[] issues)
    {
        Title = title;
        Description = description;
        Experience = experience;
        Issues = issues;

        UpdateTags(tags);
    }

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

    /// <summary>
    /// Добавить оригинальное видео к уроку.
    /// </summary>
    /// <param name="fileId">Id файла.</param>
    /// <returns>Выполненную операцию либо ошибку, если видео есть.</returns>
    public UnitResult<Error> AddOriginalVideo(Guid fileId)
    {
        var unprocessedVideo = Video.CreateUnprocessed(fileId);

        Video = unprocessedVideo;
        if (Video.OriginalFileId is null)
            return Errors.General.ValueIsRequired();

        AddDomainEvent(new LessonVideoUploadedDomainEvent(Id, Video.OriginalFileId.Value, Video.Location));
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> AddProcessedVideo(Guid fileId)
    {
        var processedVideo = Video.CreateProcessed(fileId);
        Video = processedVideo;

        if (Video.ProcessedFileId is null || Video.IsProcessed is false)
            return Errors.General.ValueIsRequired();

        return UnitResult.Success<Error>();
    }

    public void AddAutoPreview(Preview autoPreview)
    {
        AutoPreview = autoPreview;
    }

    /// <summary>
    /// Добавить задачку к уроку.
    /// </summary>
    /// <param name="issueId">Ссылка на задачу.</param>
    /// <returns>Выполненную операцию либо ошибку, что такая задача уже есть.</returns>
    public UnitResult<Error> AddIssue(Guid issueId)
    {
        if (Issues.Contains(issueId))
            return Errors.General.AlreadyExist();

        Issues = Issues.Append(issueId).ToArray();
        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// Добавить тег к уроку.
    /// </summary>
    /// <param name="tagId">Ссылка на тег.</param>
    /// <returns>Выполненную операцию либо ошибку, что такой тег уже есть.</returns>
    public UnitResult<Error> AddTag(Guid tagId)
    {
        if (Tags.Contains(tagId))
            return Errors.General.AlreadyExist();

        Tags = Tags.Append(tagId).ToArray();
        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// Удалить тег у урока.
    /// </summary>
    /// <param name="tagId">Ссылка на тег.</param>
    /// <returns>Выполненную операцию либо ошибку, что такой тег отсутствует.</returns>
    public UnitResult<Error> RemoveTag(Guid tagId)
    {
        if (!Tags.Contains(tagId))
            return Errors.General.NotFound(tagId, "tag");

        Tags = Tags.Where(id => id != tagId).ToArray();

        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// Удалить задачу у урока.
    /// </summary>
    /// <param name="issueId">Ссылка на задачу.</param>
    /// <returns>Выполненную операцию либо ошибку, что такая задача отсутствует.</returns>
    public UnitResult<Error> RemoveIssue(Guid issueId)
    {
        if (!Issues.Contains(issueId))
            return Errors.General.NotFound(issueId, "tag");

        Issues = Issues.Where(id => id != issueId).ToArray();
        return UnitResult.Success<Error>();
    }

    private void UpdateTags(IEnumerable<Guid> tags)
    {
        var updatedTags = tags.ToArray();

        var assignedTags = GetAssignedTags(updatedTags);
        if (assignedTags.Any())
            AddDomainEvent(new TagsAssignedDomainEvent(Id, assignedTags));

        var unassignedTags = GetUnassignedTags(updatedTags);
        if (unassignedTags.Any())
            AddDomainEvent(new TagsUnassignedDomainEvent(Id, unassignedTags));

        Tags = updatedTags;
    }

    private Guid[] GetAssignedTags(IEnumerable<Guid> newTags)
        => newTags.Except(Tags).ToArray();

    private Guid[] GetUnassignedTags(IEnumerable<Guid> newTags)
        => Tags.Except(newTags).ToArray();
}