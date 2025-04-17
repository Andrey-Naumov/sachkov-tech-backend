using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.Lessons.Queries.GetLessonById;

public record GetLessonByIdQuery(Guid LessonId) : IQuery;