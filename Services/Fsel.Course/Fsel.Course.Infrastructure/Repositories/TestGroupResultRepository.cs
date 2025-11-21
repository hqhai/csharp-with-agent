// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.IRepositories;

    public class TestGroupResultRepository : BaseRepository<TestGroupResult>, ITestGroupResultRepository
    {
        public TestGroupResultRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
