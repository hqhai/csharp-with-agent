// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;

    public interface IPlacementTestResultRepository : IRepository<PlacementTestResult>
    {
        Task<(EnumCourseLevel?, bool)> CheckByPassPlacementTestAsync(Guid studentId, int age);

        Task<List<Guid>> GetStudentPtIdsAsync(DateTime? startDate, DateTime? dndDate);
    }
}
