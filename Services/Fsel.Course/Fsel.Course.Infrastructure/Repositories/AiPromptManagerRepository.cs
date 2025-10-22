// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Core.Base;
    using Domain.Entities;
    using Domain.IRepositories;

    public class AiPromptManagerRepository : BaseRepository<AiPromptManager>, IAiPromptManagerRepository
    {
        public AiPromptManagerRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
