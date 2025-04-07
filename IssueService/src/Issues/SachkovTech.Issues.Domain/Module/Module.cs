using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.Module.Entities;
using SachkovTech.Issues.Domain.Module.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.Module;

public class Module : Entity<ModuleId>, ISoftDeletable
{
    private readonly List<IssuePosition> _issuesPosition = [];

    private readonly List<LessonPosition> _lessonsPosition = [];

    // ef core
    private Module(ModuleId id)
        : base(id)
    {
    }

    public Module(ModuleId moduleId, Title title, Description description)
        : base(moduleId)
    {
        Title = title;
        Description = description;
    }

    public Title Title { get; private set; } = null!;

    public Description Description { get; private set; } = null!;

    public bool IsDeleted { get; private set; }

    public DateTime? DeletionDate { get; private set; }

    public IReadOnlyList<IssuePosition> IssuesPosition => _issuesPosition.AsReadOnly();

    public IReadOnlyList<LessonPosition> LessonsPosition => _lessonsPosition.AsReadOnly();

    public int TotalIssuesCount() => _issuesPosition.Count;

    public int TotalLessonsCount() => _lessonsPosition.Count;

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

    public void UpdateMainInfo(Title title, Description description)
    {
        Title = title;
        Description = description;
    }

    public void AddIssue(IssueId issueId)
    {
        var issuePosition = new IssuePosition(
            IssuePositionId.NewIssuePositionId(),
            issueId,
            Position.Create(IssuesPosition.Count + 1).Value);

        _issuesPosition.Add(issuePosition);
    }

    public void AddLesson(LessonId lessonId)
    {
        var lessonPosition = new LessonPosition(
            LessonPositionId.NewLessonPositionId(),
            lessonId,
            Position.Create(LessonsPosition.Count + 1).Value);

        _lessonsPosition.Add(lessonPosition);
    }

    public UnitResult<Error> MoveIssue(IssuePosition issuePosition, Position newPosition)
    {
        if (!_issuesPosition.Contains(issuePosition))
            return Errors.General.NotFound();

        var result = AdjustPositionInList(_issuesPosition, issuePosition.Position, newPosition);
        if (result.IsFailure)
            return result.Error;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> MoveLesson(LessonPosition lessonPosition, Position newPosition)
    {
        if (!_lessonsPosition.Contains(lessonPosition))
            return Errors.General.NotFound();

        var result = AdjustPositionInList(_lessonsPosition, lessonPosition.Position, newPosition);
        if (result.IsFailure)
            return result.Error;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> DeleteIssuePosition(IssueId issueId)
    {
        var issuePosition = _issuesPosition.FirstOrDefault(x => x.IssueId == issueId);

        if (issuePosition == null)
            return Errors.General.NotFound();

        _issuesPosition.Remove(issuePosition);

        RecalculatePositions(_issuesPosition);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> DeleteLessonPosition(LessonId lessonId)
    {
        var lessonPosition = _lessonsPosition.FirstOrDefault(x => x.LessonId == lessonId);

        if (lessonPosition == null)
            return Errors.General.NotFound();

        _lessonsPosition.Remove(lessonPosition);

        RecalculatePositions(_lessonsPosition);

        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// Перемещает элемент в списке позиций, обновляя порядок и валидируя новые позиции.
    /// </summary>
    /// <typeparam name="T">Тип элемента, реализующий IPositionable.</typeparam>
    /// <param name="items">Список позиционируемых элементов.</param>
    /// <param name="oldPosition">Текущая позиция элемента перед перемещением.</param>
    /// <param name="newPosition">Целевая позиция для элемента.</param>
    /// <returns>Результат операции с ошибкой, если позиции недопустимы.</returns>
    private UnitResult<Error> AdjustPositionInList<T>(
        List<T> items,
        Position oldPosition,
        Position newPosition)
        where T : IPositionable
    {
        if (oldPosition == newPosition)
            return UnitResult.Success<Error>();

        if (oldPosition.Value < 1 ||
            newPosition.Value < 1 ||
            oldPosition.Value > items.Count ||
            newPosition.Value > items.Count)
        {
            return Errors.General.ValueIsInvalid(nameof(Position));
        }

        int oldIndex = oldPosition.Value - 1;
        int newIndex = newPosition.Value - 1;

        var itemToMove = items[oldIndex];
        items.RemoveAt(oldIndex);
        items.Insert(newIndex, itemToMove);

        RecalculatePositions(items);

        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// Пересчитывает позиции всех элементов в списке, начиная с 1.
    /// </summary>
    /// <typeparam name="T">Тип элемента, реализующий IPositionable.</typeparam>
    /// <param name="items">Список элементов для обновления позиций.</param>
    private void RecalculatePositions<T>(List<T> items)
        where T : IPositionable
    {
        for (int i = 0; i < items.Count; i++)
        {
            Position newPos = Position.Create(i + 1).Value;
            items[i].SetPosition(newPos);
        }
    }
}