// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class StudentRepository : BaseRepository<Student>, IStudentRepository
    {
        private readonly UserDbContext _userDbContext;
        public StudentRepository(UserDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
            _userDbContext = dbContext;
        }

        public override async Task<Student?> GetIncludeByIdAsync(Guid id)
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

        public async Task<IList<Student>> GetIncludeByIdsAsync(IList<Guid> ids)
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
    }
}
