// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories.CourseGoals
{
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Entities.CourseGoals;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.IRepositories.CourseGoals;

    public class CourseGoalConfigRepository : BaseRepository<CourseGoalConfig>, ICourseGoalConfigRepository
    {
        public CourseGoalConfigRepository(SystemDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
