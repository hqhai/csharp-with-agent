// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Core.Base;
    using Domain.Entities;
    using Domain.IRepositories;

    public class StatusStudentGoalRepository : BaseRepository<StatusStudentGoalHistory>, IStatusStudentGoalRepository
    {
        public StatusStudentGoalRepository(CourseDbContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
