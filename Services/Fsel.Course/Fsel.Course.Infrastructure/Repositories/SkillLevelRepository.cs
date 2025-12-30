// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class SkillLevelRepository : BaseRepository<SkillLevel>, ISkillLevelRepository
    {
        public SkillLevelRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper): base(dbContext, readDbContext, authContext, mapper)
        {
        }

        public async Task<IList<Skill>> GetDefaultSkillsByProgramIdAsync(Guid? programId)
        {
            if (programId == null)
            {
                return new List<Skill>();
            }

            return await Queryable
                .AsNoTracking()
                .Where(sl => sl.Level!.ProgramId == programId)
                .Select(sl => sl.Skill!)
                .Distinct()
                .ToListAsync();
        }
    }
}
