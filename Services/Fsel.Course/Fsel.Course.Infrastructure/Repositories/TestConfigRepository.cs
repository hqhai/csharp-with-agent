// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities.TestConfig;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.TestConfig;
    using Fsel.Course.Infrastructure.Common;
    using Microsoft.EntityFrameworkCore;

    public class TestConfigRepository : BaseRepository<TestConfig>, ITestConfigRepository
    {
        public TestConfigRepository(CourseDbContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
