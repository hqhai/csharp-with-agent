// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories.CourseGoals
{
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities.CourseGoals;
    using Fsel.System.Domain.IRepositories.CourseGoals;

    public class CourseGoalRepository : BaseRepository<CourseGoal>, ICourseGoalRepository
    {
        public CourseGoalRepository(SystemDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
