// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Shared.Enums;
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
                var query = await Queryable.FirstOrDefaultAsync(x => x.Id == id);

                if (query != null && query.Level == EnumPlacementTestLevel.IELTS)
                {
                    query = await Queryable.Include(x => x.PlacementTestSections.Where(n => n.SectionGroup != null))
                                .ThenInclude(x => x.SectionGroup)
                                .ThenInclude(x => x!.Sections)
                                .ThenInclude(x => x.SectionParts)
                                .ThenInclude(x => x.SectionQuestions.Where(n => n.Question != null))
                                .ThenInclude(x => x.Question)
                                .FirstOrDefaultAsync(x => x.Id == id);
                }
                else
                {
                    query = await Queryable
                                .Include(x => x.PlacementTestSections.Where(n => n.SectionGroup != null))
                                .ThenInclude(x => x.SectionGroup)
                                .ThenInclude(x => x!.Sections)
                                .ThenInclude(x => x.SectionQuestions.Where(n => n.Question != null))
                                .ThenInclude(x => x.Question)
                                .FirstOrDefaultAsync(x => x.Id == id);
                }

                return query;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
