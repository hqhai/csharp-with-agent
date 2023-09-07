// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using EntityCourse = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Domain.IRepositories
{
    public interface ICourseRepository : IRepository<EntityCourse>
    {
        Task<EntityCourse?> GetIncludeLessonVideoByIdAsync(Guid id);

        Task<EntityCourse?> GetIncludeCourseUnitMockTestByIdAsync(Guid id);
    }
}
