// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class ClassLiveWorkFlowRepository : BaseRepository<ClassLiveWorkFlow>, IClassLiveWorkFlowRepository
    {
        public ClassLiveWorkFlowRepository(TrainingDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public override async Task<ClassLiveWorkFlow?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            return await Queryable.Include(x => x.ClassLiveCalendar).ThenInclude(p => p == null ? null : p.Class).ThenInclude(i => i == null ? null : i.ClassStudents).FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
