// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.IRepositories;

    public class ClassLiveWorkFlowRepository : BaseRepository<ClassLiveWorkFlow>, IClassLiveWorkFlowRepository
    {
        public ClassLiveWorkFlowRepository(TrainingDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
