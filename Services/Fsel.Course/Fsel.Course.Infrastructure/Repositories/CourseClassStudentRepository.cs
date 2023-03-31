// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class CourseClassStudentRepository : BaseRepository<CourseClassStudent>, ICourseClassStudentRepository
    {
        public CourseClassStudentRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public async Task<CourseClassStudent?> GetIncludeByIdsAsync(IEnumerable<Guid>? ids, Guid courseId)
        {
            try
            {
                return await Queryable.Where(e => e.CourseId == courseId)
                                      .Where(e => ids != null && ids.Contains(e.ClassId))
                                      .FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
