// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;

    public class PackageRepository : BaseRepository<Package>, IPackageRepository
    {
        public PackageRepository(OrderingDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
