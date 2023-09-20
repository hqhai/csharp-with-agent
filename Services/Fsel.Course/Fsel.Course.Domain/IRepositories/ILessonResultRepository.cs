// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;

    public interface ILessonResultRepository : IRepository<LessonResult>
    {
        Task<List<LessonResult>?> GetsByIds(IList<Guid>? ids);

        Task<List<LessonResult>?> GetsByLessonIds(IList<Guid>? lessonIds, Guid? studentId);

        Task<LessonResult?> GetByLessonId(Guid? lessonId, Guid? studentId);

        Task<List<LessonResult>?> GetByCourseResult(CourseResultModel courseResult);
    }
}
