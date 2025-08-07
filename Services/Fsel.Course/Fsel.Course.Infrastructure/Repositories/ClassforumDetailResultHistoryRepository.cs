// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;

    public class ClassforumDetailResultHistoryRepository : BaseRepository<ClassForumDetailResultHistory>, IClassforumDetailResultHistoryRepository
    {
        public ClassforumDetailResultHistoryRepository(CourseDbContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
