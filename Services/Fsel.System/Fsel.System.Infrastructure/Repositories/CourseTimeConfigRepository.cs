// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities.Configs;
    using Fsel.System.Domain.IRepositories;

    public class CourseTimeConfigRepository : BaseRepository<CourseTimeConfig>, ICourseTimeConfigRepository
    {
        public CourseTimeConfigRepository(SystemDbContext dbContext, SystemReadDbContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
