// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;

    public interface IMockTestRepository : IRepository<MockTest>
    {
        Task<bool> IsUnitSkillMockTest(Guid Id);
    }
}
