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

        public async Task<Category?> GetSecondLevelFromRootAsync(Guid? programId, CancellationToken cancellationToken)
        {
            if (!programId.HasValue)
            {
                return null;
            }
            var current = await ReadQueryable.FirstOrDefaultAsync(x => x.Id == programId, cancellationToken);

            while (current != null)
            {
                var parent = await ReadQueryable
                     .FirstOrDefaultAsync(x => x.Id == current.ParentId, cancellationToken);
                if (parent != null && parent.ParentId == null)
                {
                    break;
                }
                else
                {
                    current = await ReadQueryable.FirstOrDefaultAsync(x => x.Id == current.ParentId, cancellationToken);
                }
            }

            return current;
        }

        public async Task<Category?> GetRootSubjectAsync(Guid? programId, CancellationToken cancellationToken)
        {
            if (!programId.HasValue)
            {
                return null;
            }

            var current = await ReadQueryable.FirstOrDefaultAsync(x => x.Id == programId, cancellationToken);

            while (current != null && current.ParentId != null)
            {
                current = await ReadQueryable.FirstOrDefaultAsync(x => x.Id == current.ParentId, cancellationToken);
            }

            return current; // đây chính là node có ParentId == null
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
