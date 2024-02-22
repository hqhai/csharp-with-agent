// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;

namespace Fsel.Course.Domain.IRepositories
{
    public interface ILessonRepository : IRepository<Lesson>
    {
        Task<bool> IsLessonUsed(Guid id);

        Task<Lesson?> GetIncludeVideoByIdAsync(Guid id);

        Task<double> GetPercentLesson(Guid courseId, Guid unitId, Guid? studentId);

        Task<double> GetPercentClassForum(Guid courseId, Guid unitId, Guid? studentId);

        Task<double> GetPercentHomeWork(Guid courseId, Guid unitId, Guid? studentId);

        Task<Lesson?> GetIncludeByIdNoTrackingAsync(Guid id, int? siteId = null);

        Task<Lesson?> GetAsync(Guid? lessonId);
    }
}
