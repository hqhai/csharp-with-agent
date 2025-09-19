// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using EntityRubyText = Fsel.Course.Domain.Entities.RubyText;

    public class RubyTextRepository : BaseRepository<EntityRubyText>, IRubyTextRepository
    {
        public RubyTextRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper) : base
            (dbContext, readDbContext, authContext, mapper)
        {

        }
    }
}
