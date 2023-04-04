// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;

namespace Fsel.Course.Domain.IRepositories
{
    public interface ICourseUnitMockTestRepository : IRepository<CourseUnitMockTest>
    {
        Task<List<CourseUnitMockTest>> GetListByUnitIdAsync(Guid Id);
    }
}
