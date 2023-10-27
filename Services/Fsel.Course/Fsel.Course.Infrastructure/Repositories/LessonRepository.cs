// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class LessonRepository : BaseRepository<Lesson>, ILessonRepository
    {
        public LessonRepository(CourseDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public async Task<Lesson?> GetIncludeByIdNoTrackingAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await Queryable.Include(e => e.UnitLessons.Where(n => !n.IsDeleted))
                                 .Include(e => e.ClassForum)
                                 .ThenInclude(e => e!.ClassForumFiles.OrderBy(x => x.CreatedDate))
                                 .Include(e => e.LessonHomeWorks.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .ThenInclude(e => e.HomeWork)
                                 .Include(e => e.LessonExtraPractices.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .Include(e => e.LessonInstructions.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .Include(e => e.LessonVideos.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .ThenInclude(e => e.Video)
                                 .ThenInclude(e => e!.VideoTimeCodes.OrderBy(x => x.CreatedDate))
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<double> GetPercentHomeWork(Guid unitId, Guid? studentId)
        {
            var lessons = await Queryable.Include(x => x.LessonResults.Where(x => x.UnitId == unitId && x.StudentId == studentId))
                                    .ThenInclude(x => x.HomeWorkResults.Where(x => x.StudentId == studentId))
                                 .Include(x => x.LessonHomeWorks)
                                 .Include(x => x.UnitLessons)
                                 .Where(x => x.UnitLessons.Any(x => x.UnitId == unitId)).ToListAsync();

            var listDones = lessons.Select(x => new
            {
                CountDone = x.LessonResults.SelectMany(x => x.HomeWorkResults).Where(x => x.Status == EnumResultStatus.Done).Count(),
                TotalDone = x.LessonHomeWorks.Count
            }).ToList();
            return NumberHelper.GetPercent(listDones.Sum(x => x.CountDone), listDones.Sum(x => x.TotalDone));
        }

        public async Task<double> GetPercentClassForum(Guid unitId, Guid? studentId)
        {
            var lessons = await Queryable.Include(x => x.LessonResults.Where(x => x.UnitId == unitId && x.StudentId == studentId))
                                   .ThenInclude(x => x.ClassForumResults.Where(x => x.StudentId == studentId))
                                .Include(x => x.ClassForum)
                                .Include(x => x.UnitLessons)
                                .Where(x => x.UnitLessons.Any(x => x.UnitId == unitId)).ToListAsync();

            var listDones = lessons.Select(x => new
            {
                CountDone = x.LessonResults.SelectMany(x => x.ClassForumResults).Where(x => x.Status == EnumClassForumResultStatus.PendingForGrading || x.Status == EnumClassForumResultStatus.Graded).Count(),
                TotalDone = 1
            }).ToList();
            return NumberHelper.GetPercent(listDones.Sum(x => x.CountDone), listDones.Sum(x => x.TotalDone));
        }

        public async Task<double> GetPercentLesson(Guid unitId, Guid? studentId)
        {
            var lessons = await Queryable.Include(x => x.UnitLessons)
                                 .Include(x => x.LessonResults.Where(x => x.UnitId == unitId && x.StudentId == studentId))
                                 .Where(x => x.UnitLessons.Any(x => x.UnitId == unitId)).ToListAsync();

            var listDones = lessons.Select(x => new
            {
                CountDone = x.LessonResults.Where(x => x.Status == EnumResultStatus.Done).Count(),
                TotalDone = 1
            }).ToList();
            return NumberHelper.GetPercent(listDones.Sum(x => x.CountDone), listDones.Sum(x => x.TotalDone));
        }

        public override async Task<Lesson?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await Queryable.Include(e => e.UnitLessons.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .Include(e => e.ClassForum)
                                 .ThenInclude(e => e!.ClassForumFiles.OrderBy(x => x.CreatedDate))
                                 .Include(x => x.LessonResults.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .Include(e => e.LessonHomeWorks.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .ThenInclude(e => e.HomeWork)
                                 .Include(e => e.LessonExtraPractices.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .Include(e => e.LessonInstructions.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .Include(e => e.LessonVideos.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .ThenInclude(x => x.Video)
                                 .ThenInclude(x => x!.VideoTimeCodes.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Lesson?> GetIncludeVideoByIdAsync(Guid id)
        {
            try
            {
                return await Queryable.Include(x => x.LessonResults.Where(n => !n.IsDeleted))
                                      .Include(x => x.LessonVideos.Where(n => !n.IsDeleted))
                                      .ThenInclude(x => x.Video)
                                      .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> IsLessonUsed(Guid id)
        {
            return await Queryable
                 .Include(x => x.UnitLessons.Where(n => !n.IsDeleted))
                 .AnyAsync(x => x.Id == id && x.UnitLessons.Count > 0);
        }
    }
}
