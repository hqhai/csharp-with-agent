// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Core.Base;
    using Domain.Entities;
    using Domain.IRepositories;

    public class AiFeatureConfigRepository  : BaseRepository<AIFeatureConfig>, IAiFeatureConfigRepository
    {
        public AiFeatureConfigRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
