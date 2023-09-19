// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class LessonResultRepository : BaseRepository<LessonResult>, ILessonResultRepository
    {
        public LessonResultRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public async Task<List<LessonResult>?> GetsByIds(IList<Guid>? ids)
        {
            if (ids == null || !ids.Any())
            {
                return default;
            }
            return await Queryable.Include(x => x.ClassForumResults.Where(x => ids.Contains(x.LessonResultId)))
                                                     .Include(x => x.VideoResult)
                                                     .Include(x => x.HomeWorkResults.Where(x => ids.Contains(x.LessonResultId)))
                                                     .Where(x => ids.Contains(x.Id))
                                                     .ToListAsync();
        }

        public async Task<List<LessonResult>?> GetsByLessonIds(IList<Guid>? lessonIds, Guid? studentId)
        {
            if (lessonIds == null || !lessonIds.Any())
            {
                return default;
            }
            return await Queryable.Include(x => x.ClassForumResults.Where(x => x.StudentId == studentId))
                                                        .Include(x => x.VideoResult)
                                                     .Include(x => x.HomeWorkResults.Where(x => x.StudentId == studentId))
                                                        .Where(x => lessonIds.Contains(x.LessonId) && x.StudentId == studentId)
                                                     .ToListAsync();
        }
    }
}
