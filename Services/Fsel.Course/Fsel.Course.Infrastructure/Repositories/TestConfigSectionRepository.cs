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
    using Fsel.Course.Domain.Entities.TestConfig;
    using Fsel.Course.Domain.IRepositories;

    public class TestConfigSectionRepository : BaseRepository<TestConfigSection>, ITestConfigSectionRepository
    {
        public TestConfigSectionRepository(CourseDbContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
