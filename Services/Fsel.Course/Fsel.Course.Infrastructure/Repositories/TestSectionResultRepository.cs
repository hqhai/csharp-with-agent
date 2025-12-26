// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Core.Base;
    using Domain.Entities.TestConfigs;
    using Domain.IRepositories;

    public class TestSectionResultRepository : BaseRepository<TestSectionResult>, ITestSectionResultRepository
    {
        public TestSectionResultRepository(BaseDbContext masterDbContext, BaseDbContext readOnlyDbContext, AuthContext authContext, IMapper mapper) : base(masterDbContext, readOnlyDbContext, authContext, mapper)
        {
        }
    }
}
