// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;

    public interface ILessonResultRepository : IRepository<LessonResult>
    {
        Task<List<LessonResult>?> GetListAsync(IList<Guid>? ids);

        Task<List<LessonResult>?> GetListAsync(IList<Guid>? lessonIds, Guid? studentId);

        Task<LessonResult?> GetAsync(Guid? lessonId, Guid? studentId);

        Task<List<LessonResult>?> GetListAsync(CourseResultModel courseResult);

        Task<LessonResult?> GetAsync(Guid? studentId, Guid courseId);
    }
}
