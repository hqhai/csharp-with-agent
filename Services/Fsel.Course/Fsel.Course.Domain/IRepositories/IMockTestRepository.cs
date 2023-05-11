// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels;

    public interface IMockTestRepository : IRepository<MockTest>
    {
        Task<bool> IsUnitSkillMockTest(Guid id);

        Task<bool> IsCourseUnitMockTest(Guid id);

        Task<MockTestModel?> GetIncludeAllAsync(Guid? id);

        IQueryable<MockTestModel> SearchAsync(EnumMockTestType? mockTestType);
    }
}
