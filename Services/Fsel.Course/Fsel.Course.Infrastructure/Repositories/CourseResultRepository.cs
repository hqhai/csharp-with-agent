// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;
    using AutoMapper;

    public class CourseResultRepository : BaseRepository<CourseResult>, ICourseResultRepository
    {
        public override IQueryable<CourseResult> Queryable => base.Queryable.Where(x => x.WorkingStatus != Shared.Enums.EnumWorkingStatus.NotWorking);

        public CourseResultRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper): base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
