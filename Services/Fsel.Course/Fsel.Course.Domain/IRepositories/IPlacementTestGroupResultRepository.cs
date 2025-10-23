// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;

    public interface IPlacementTestGroupResultRepository : IRepository<PlacementTestGroupResult>
    {
        Task<IList<Guid>> GetStudentIdsAsync(IList<EnumCompletionStatus>? completionStatuses, IList<Guid> studentIds, IList<EnumCourseLevel>? suggetLevels, IList<EnumCourseLevel>? courseLevels);
    }
}
