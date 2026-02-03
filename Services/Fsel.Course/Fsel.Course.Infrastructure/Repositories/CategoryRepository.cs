// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }

        public async Task<Category?> GetProgramLevelsAsync(Guid? programId, CancellationToken cancellationToken)
        {
            if (!programId.HasValue)
            {
                return null;
            }
            return await ReadQueryable.Include(x => x.Levels)
                                      .FirstOrDefaultAsync(x => x.Id == programId.Value, cancellationToken);
        }
    }
}
