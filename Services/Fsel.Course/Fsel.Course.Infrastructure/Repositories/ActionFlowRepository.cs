// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.IRepositories;

    public class ActionFlowRepository : BaseRepository<ActionFlow>, IActionFlowRepository
    {
        public ActionFlowRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
