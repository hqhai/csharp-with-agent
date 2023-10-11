// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class TeacherRepository : BaseRepository<Teacher>, ITeacherRepository
    {
        public TeacherRepository(UserDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public override async Task<Teacher?> GetIncludeByIdAsync(Guid id, int? siteId = null)
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

        public async Task<IList<Teacher>> GetIncludeByIdsAsync(IList<Guid> ids, int? siteId = null)
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

        public async Task<Teacher?> GetIncludeByUserIdAsync(Guid userId, int? siteId = null)
        {
            try
            {
                return await Queryable
                .Include(x => x.Human)
                .FirstOrDefaultAsync(x => x.Human!.UserId == userId);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
