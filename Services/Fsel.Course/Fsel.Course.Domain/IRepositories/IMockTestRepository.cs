// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;

    public interface IMockTestRepository : IRepository<MockTest>
    {
        Task<bool> IsUnitSkillMockTest(Guid id);

        Task<bool> IsCourseFullMockTest(Guid id);

        Task<MockTestModel?> GetIncludeAllAsync(Guid? id);

        Task<MockTestModel?> GetIncludeAsync(Guid courseId, Guid unitId, Guid? studentId);
    }
}
