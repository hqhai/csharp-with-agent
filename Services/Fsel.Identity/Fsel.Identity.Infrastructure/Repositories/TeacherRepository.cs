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

        public override async Task<Teacher?> GetIncludeByIdAsync(Guid id)
        {
            try
            {
                return await Queryable
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IList<Teacher>> GetIncludeByIdsAsync(IList<Guid> ids)
        {
            try
            {
                return await Queryable
                .Include(x => x.User)
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Teacher?> GetIncludeByUserIdAsync(Guid userId)
        {
            try
            {
                return await Queryable
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.UserId == userId);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
