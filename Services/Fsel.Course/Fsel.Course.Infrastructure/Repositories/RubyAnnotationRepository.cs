// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;
    using EntityRubyText = Fsel.Course.Domain.Entities.RubyAnnotation;

    public class RubyAnnotationRepository : BaseRepository<EntityRubyText>, IRubyAnnotationRepository
    {
        public RubyAnnotationRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper) : base
            (dbContext, readDbContext, authContext, mapper)
        {

        }
    }
}
