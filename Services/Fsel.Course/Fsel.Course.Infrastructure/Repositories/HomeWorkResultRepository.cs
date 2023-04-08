// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class HomeWorkResultRepository : BaseRepository<HomeWorkResult>, IHomeWorkResultRepository
    {
        public HomeWorkResultRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public async Task<bool> GetCheckByIdsAsync(IEnumerable<Guid>? ids)
        {
            try
            {
                return await Queryable.AnyAsync(e => ids != null && ids.Contains(e.Id));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
