// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.IRepositories;
    using AutoMapper;

    public class TestAISettingRepository : BaseRepository<TestAISetting>, ITestAISettingRepository
    {
        public TestAISettingRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper): base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
