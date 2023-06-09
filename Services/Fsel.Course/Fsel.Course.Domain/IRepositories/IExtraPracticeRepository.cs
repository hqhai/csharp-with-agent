// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Domain.IRepositories
{
    public interface IExtraPracticeRepository : IRepository<ExtraPractice>
    {
        Task<ExtraPracticeModel?> GetIncludeAllAsync(Guid? id);
    }
}
