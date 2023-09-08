// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class StudentRepository : BaseRepository<Student>, IStudentRepository
    {
        public StudentRepository(UserDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public override async Task<Student?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await Queryable
                .Include(x => x.Human)
                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IList<Student>> GetIncludeByIdsAsync(IList<Guid> ids, int? siteId = null)
        {
            try
            {
                return await Queryable
                .Include(x => x.Human)
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
