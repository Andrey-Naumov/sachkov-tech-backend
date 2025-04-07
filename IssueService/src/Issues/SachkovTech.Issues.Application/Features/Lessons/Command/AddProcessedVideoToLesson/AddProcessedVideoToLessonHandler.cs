using CSharpFunctionalExtensions;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.Lesson.ValueObjects;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.Lessons.Command.AddProcessedVideoToLesson;

public class AddProcessedVideoToLessonHandler : ICommandHandler<AddProcessedVideoToLessonCommand>
{
    private readonly ILessonsRepository _lessonsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddProcessedVideoToLessonHandler(ILessonsRepository lessonsRepository, IUnitOfWork unitOfWork)
    {
        _lessonsRepository = lessonsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UnitResult<ErrorList>> Handle(AddProcessedVideoToLessonCommand command, CancellationToken cancellationToken = default)
    {
        var lesson = await _lessonsRepository.GetById(command.LessonId, cancellationToken);
        if (lesson.IsFailure)
            return lesson.Error.ToErrorList();

        var autoPreview = new Preview(command.PreviewId);

        var addVideoResult = lesson.Value.AddProcessedVideo(command.ProcessedVideoId);
        if (addVideoResult.IsFailure)
            return addVideoResult.Error.ToErrorList();

        lesson.Value.AddAutoPreview(autoPreview);

        await _unitOfWork.SaveChanges(cancellationToken);

        return UnitResult.Success<ErrorList>();
    }
}