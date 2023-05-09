// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class PlacementTestRepository : BaseRepository<PlacementTest>, IPlacementTestRepository
    {
        public PlacementTestRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public override async Task<PlacementTest?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await Queryable
                .Include(x => x.PlacementTestSections.Where(n => !n.IsDeleted && n.SectionGroup != null))
                .ThenInclude(x => x.SectionGroup)
                .ThenInclude(x => x.Sections.Where(n => !n.IsDeleted))
                .ThenInclude(x => x.SectionParts.Where(n => !n.IsDeleted))
                .ThenInclude(x => x.SectionQuestions.Where(n => !n.IsDeleted && n.Question != null))
                .ThenInclude(x => x.Question)
                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
