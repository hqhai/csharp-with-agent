// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;

    public class SectionGroupResultRepository : BaseRepository<SectionGroupResult>, ISectionGroupResultRepository
    {
        public SectionGroupResultRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper): base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
