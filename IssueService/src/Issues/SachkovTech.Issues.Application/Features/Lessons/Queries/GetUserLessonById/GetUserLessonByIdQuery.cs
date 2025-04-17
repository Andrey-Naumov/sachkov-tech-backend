using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.Lessons.Queries.GetUserLessonById;

public record GetUserLessonByIdQuery(Guid LessonId, Guid UserId) : IQuery;