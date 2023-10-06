// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Microsoft.EntityFrameworkCore;

    public class LessonResultRepository : BaseRepository<LessonResult>, ILessonResultRepository
    {
        public LessonResultRepository(CourseDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public async Task<List<LessonResult>?> GetListAsync(IList<Guid>? ids)
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

        public async Task<LessonResult?> GetAsync(Guid? lessonId, Guid? studentId)
        {
            return await Queryable.Include(x => x.ClassForumResults.Where(x => x.StudentId == studentId))
                                    .Include(x => x.VideoResult)
                                    .Include(x => x.Lesson)
                                    .Include(x => x.HomeWorkResults.Where(x => x.StudentId == studentId))
                                    .FirstOrDefaultAsync(x => x.LessonId == lessonId && x.StudentId == studentId);
        }

        public async Task<List<LessonResult>?> GetListAsync(IList<Guid>? lessonIds, Guid? studentId)
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

        public async Task<List<LessonResult>?> GetListAsync(CourseResultModel courseResult)
        {
            return await Queryable.Include(x => x.ClassForumResults.Where(x => x.StudentId == courseResult.StudentId))
                                     .Include(x => x.VideoResult)
                                     .Include(x => x.HomeWorkResults.Where(x => x.StudentId == courseResult.StudentId))
                                     .Where(x => x.CourseId == courseResult.CourseId && x.StudentId == courseResult.StudentId).ToListAsync();
        }
    }
}
