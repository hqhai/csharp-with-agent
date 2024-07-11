// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.IRepositories
{
    using Fsel.System.Domain.Entities;

    public interface ICrmLocationRepository
    {
        IQueryable<CrmLocation> Queryable { get; }
    }
}
