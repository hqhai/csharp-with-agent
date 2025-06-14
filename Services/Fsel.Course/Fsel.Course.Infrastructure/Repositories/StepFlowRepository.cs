// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.IRepositories;

    public class StepFlowRepository : BaseRepository<StepFlow>, IStepFlowRepository
    {
        public StepFlowRepository(CourseDbContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
