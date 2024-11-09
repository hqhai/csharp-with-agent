// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;

    public interface IPlacementTestGroupResultRepository : IRepository<PlacementTestGroupResult>
    {
        Task<IList<Guid>> GetStudentIdsAsync(EnumCompletionStatus? status, IList<Guid> studentIds, bool isCheckDate, EnumCourseLevel? currentLevel, EnumCourseLevel? courseLevel);
    }
}
