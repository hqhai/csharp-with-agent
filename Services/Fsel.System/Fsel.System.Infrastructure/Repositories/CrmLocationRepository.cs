// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using global::System.Collections.Generic;
    using global::System.Linq;

    public class CrmLocationRepository : ICrmLocationRepository
    {
        private readonly CrmDbContext _dbContext;

        public CrmLocationRepository(CrmDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<CrmLocation> Queryable => _dbContext.Locations;
    }
}
