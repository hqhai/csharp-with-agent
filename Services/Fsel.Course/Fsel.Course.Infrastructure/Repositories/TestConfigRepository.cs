// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities.TestConfig;
    using Fsel.Course.Domain.IRepositories;

    public class TestConfigRepository : BaseRepository<TestConfig>, ITestConfigRepository
    {
        public TestConfigRepository(CourseDbContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
