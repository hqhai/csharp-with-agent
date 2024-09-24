// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;

    public interface IFinalTestRepository : IRepository<FinalTest>
    {
        Task<FinalTestModel?> GetIncludeAllAsync(Guid? id);
    }
}
