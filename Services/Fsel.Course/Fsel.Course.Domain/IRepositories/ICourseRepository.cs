// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Enums;
using EntityCourse = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Domain.IRepositories
{
    public interface ICourseRepository : IRepository<EntityCourse>
    {
        Task<EntityCourse?> GetIncludeLessonVideoByIdAsync(Guid id);

        Task<EntityCourse?> GetIncludeCourseUnitMockTestByIdAsync(Guid id);

        Task<EntityCourse?> GetIncludeCourseResult(Guid id, Guid? studentId);

        Task<(double, double, int, int)> GetContentCompleted(Guid courseId, EnumCourseType courseType, Guid? studentId);
    }
}
