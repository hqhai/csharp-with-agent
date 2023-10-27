// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class ClassLiveWorkFlowRepository : BaseRepository<ClassLiveWorkFlow>, IClassLiveWorkFlowRepository
    {
        public ClassLiveWorkFlowRepository(TrainingDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public override async Task<ClassLiveWorkFlow?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            return await Queryable.Include(x => x.ClassLiveCalendar)
                                    .ThenInclude(p => p!.Class)
                                    .ThenInclude(i => i!.ClassStudents)
                                    .Include(x => x.ClassLiveWorkFlowPlans)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
