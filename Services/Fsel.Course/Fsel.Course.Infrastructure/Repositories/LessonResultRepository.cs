// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Microsoft.EntityFrameworkCore;
    using AutoMapper;

    public class LessonResultRepository : BaseRepository<LessonResult>, ILessonResultRepository
    {
        public LessonResultRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }

        public async Task<List<LessonResult>?> GetListAsync(IList<Guid>? ids)
        {
            if (ids == null || !ids.Any())
            {
                return default;
            }
            return await Queryable.Include(x => x.ClassForumResults.Where(x => ids.Contains(x.LessonResultId)))
                                    .Include(x => x.VideoResults)
                                    .Include(x => x.HomeWorkResults.Where(x => ids.Contains(x.LessonResultId)))
                                    .Where(x => ids.Contains(x.Id))
                                    .ToListAsync();
        }

        public async Task<LessonResult?> GetAsync(Guid? courseId, Guid? unitId, Guid? lessonId, Guid? studentId)
        {
            return await Queryable.Include(x => x.ClassForumResults.Where(x => x.StudentId == studentId))
                                    .Include(x => x.VideoResults)
                                    .Include(x => x.Lesson)
                                    .Include(x => x.HomeWorkResults.Where(x => x.StudentId == studentId))
                                    .Where(x => x.CourseId == courseId && x.UnitId == unitId)
                                    .FirstOrDefaultAsync(x => x.LessonId == lessonId && x.StudentId == studentId);
        }

        public async Task<LessonResult?> GetAsync(Guid? studentId, Guid courseId)
        {
            return await Queryable.Include(x => x.Lesson).ThenInclude(x => x!.LessonInstructions)
                                                        .Include(x => x.VideoResults)
                                                        .Include(x => x.HomeWorkResults)
                                                        .Include(x => x.ClassForumResults)
                                                        .Where(x => x.Status != EnumResultStatus.New && x.CourseId == courseId)
                                                        .Where(x => x.StudentId == studentId && x.Status != EnumResultStatus.Unfinished)
                                                        .OrderByDescending(x => x.CreatedDate)
                                                        .ThenBy(x => x.UpdatedDate)
                                                        .AsNoTracking()
                                                        .FirstOrDefaultAsync();
        }

        public async Task<List<LessonResult>?> GetListAsync(IList<Guid>? lessonIds, Guid courseId, Guid unitId, Guid? studentId)
        {
            if (lessonIds == null || !lessonIds.Any())
            {
                return default;
            }
            return await Queryable.Include(x => x.ClassForumResults)
                                .Include(x => x.VideoResults)
                                .Include(x => x.HomeWorkResults)
                                .Where(x => x.UnitId == unitId && x.CourseId == courseId)
                                .Where(x => lessonIds.Contains(x.LessonId) && x.StudentId == studentId)
                                .AsNoTracking()
                                .ToListAsync();
        }

        public async Task<List<LessonResult>?> GetListAsync(CourseResultModel courseResult)
        {
            return await Queryable.Include(x => x.ClassForumResults.Where(x => x.StudentId == courseResult.StudentId))
                                     .Include(x => x.VideoResults)
                                     .Include(x => x.HomeWorkResults.Where(x => x.StudentId == courseResult.StudentId))
                                     .Where(x => x.CourseId == courseResult.CourseId && x.StudentId == courseResult.StudentId).ToListAsync();
        }
    }
}
