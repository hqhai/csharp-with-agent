// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;

    public class CourseTeacherRepository : BaseRepository<CourseTeacher>, ICourseTeacherRepository
    {
        public CourseTeacherRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
