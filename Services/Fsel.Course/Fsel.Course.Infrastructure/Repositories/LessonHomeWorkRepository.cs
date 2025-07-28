// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using AutoMapper;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class LessonHomeWorkRepository : BaseRepository<LessonHomeWork>, ILessonHomeWorkRepository
    {
        public LessonHomeWorkRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper): base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
