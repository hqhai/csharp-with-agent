// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Infrastructure.Repositories
{
    using Fsel.Master.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class MasterBaseRepository<T> : IMasterBaseRepository<T> where T : class
    {
        protected readonly MasterDBContext _context;
        protected readonly DbSet<T> _dbSet;

        public MasterBaseRepository(MasterDBContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public IQueryable<T> Queryable => _dbSet.AsNoTracking();
    }
}
