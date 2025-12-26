// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Entities.V1i1;

namespace Fsel.Course.Domain.IRepositories
{
    public interface ILessonRepository : IRepository<Lesson>
    {
        Task<bool> IsLessonUsed(Guid id);

        Task<Lesson?> GetIncludeVideoByIdAsync(Guid id);

        Task<double> GetPercentLesson(Guid courseId, Guid unitId, Guid? studentId);

        Task<double> GetPercentClassForum(Guid courseId, Guid unitId, Guid? studentId);

        Task<double> GetPercentHomeWork(Guid courseId, Guid unitId, Guid? studentId);

        Task<Lesson?> GetIncludeByIdNoTrackingAsync(Guid id);

        Task<Lesson?> GetAsync(Guid? lessonId);

        Task<(IDictionary<Guid, (Lesson, LessonResult)>, IDictionary<Guid, Lesson>)> BuildLessonLookupsAsync(UnitResult unitResult, IList<UnitModule> unitModules);
    }
}
