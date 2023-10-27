// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Domain.IRepositories
{
    public interface IPlacementTestRepository : IRepository<PlacementTest>
    {
        Task<PlacementTestModel?> GetIncludePlacementTestById(Guid id);
    }
}
