using CSharpFunctionalExtensions;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.Lessons.Command.DeleteTagFromLesson;

public class DeleteTagFromLessonHandler : ICommandHandler<DeleteTagFromLessonCommand>
{
    private readonly ILessonsRepository _lessonsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTagFromLessonHandler(
        ILessonsRepository lessonsRepository,
        IUnitOfWork unitOfWork)
    {
        _lessonsRepository = lessonsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UnitResult<ErrorList>> Handle(
        DeleteTagFromLessonCommand command,
        CancellationToken cancellationToken)
    {
        var lessons = await _lessonsRepository.GetLessonsByTagId(command.TagId, cancellationToken);
        if (lessons.IsFailure)
            return Errors.General.NotFound().ToErrorList();

        foreach (var lesson in lessons.Value)
        {
            var result = lesson.RemoveTag(command.TagId);

            if (result.IsFailure)
                return Errors.General.NotFound().ToErrorList();
        }

        await _unitOfWork.SaveChanges(cancellationToken);

        return UnitResult.Success<ErrorList>();
    }
}