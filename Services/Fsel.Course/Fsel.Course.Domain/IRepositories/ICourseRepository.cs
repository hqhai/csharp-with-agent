// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Models.EntityModels;
using EntityCourse = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Domain.IRepositories
{
    public interface ICourseRepository : IRepository<EntityCourse>
    {
        Task<EntityCourse?> GetIncludeLessonVideoByIdAsync(Guid id);

        Task<EntityCourse?> GetIncludeCourseUnitMockTestByIdAsync(Guid id, Guid? studentId);

        Task<EntityCourse?> GetAsync(Guid id, Guid? studentId);

        Task<CourseModel?> GetIncludeCourseResult(Guid id, Guid? studentId, string? classCode);

        Task<(int, int)> GetDisplayOrder(CourseResultModel courseResult);
    }
}
