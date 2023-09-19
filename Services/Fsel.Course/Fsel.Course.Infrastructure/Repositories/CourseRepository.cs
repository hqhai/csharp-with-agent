// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using EntityCourse = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class CourseRepository : BaseRepository<EntityCourse>, ICourseRepository
    {
        private readonly IUnitRepository _unitRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public CourseRepository(CourseDbContext dbContext, AuthContext authContext, IUnitRepository unitRepository, ILessonResultRepository lessonResultRepository) : base(dbContext, authContext)
        {
            _unitRepository = unitRepository;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<bool> IsUnitUsed(Guid id)
        {
            return await Queryable
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted))
                .AnyAsync(x => x.Id == id && x.CourseUnitMockTests.Count > 0);
        }

        public override async Task<EntityCourse?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await Queryable
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted).OrderBy(x => x.DisplayOrder))
                .ThenInclude(x => x.Unit)
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted).OrderBy(x => x.DisplayOrder))
                .ThenInclude(x => x.FinalTest)
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted).OrderBy(x => x.DisplayOrder))
                .ThenInclude(x => x.MockTest)
                .Include(x => x.CourseTeachers.Where(c => !c.IsDeleted).OrderBy(x => x.CreatedDate))
                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<EntityCourse?> GetIncludeLessonVideoByIdAsync(Guid id)
        {
            try
            {
                return await Queryable.Include(x => x.CourseResults)
                                        .Include(x => x.CourseUnitMockTests.Where(y => y.IsDeleted == false).OrderBy(x => x.DisplayOrder))
                                        .ThenInclude(x => x.Unit)
                                        .ThenInclude(x => x!.UnitLessons.Where(y => y.IsDeleted == false).OrderBy(x => x.DisplayOrder))
                                        .ThenInclude(x => x.Lesson)
                                        .ThenInclude(x => x!.LessonVideos.Where(y => y.IsDeleted == false).OrderBy(x => x.CreatedDate))
                                        .Where(x => x.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<EntityCourse?> GetIncludeCourseUnitMockTestByIdAsync(Guid id)
        {
            try
            {
                return await Queryable.Include(x => x.CourseUnitMockTests)
                                                  .Include(x => x.UnitResults)
                                                  .Include(x => x.MockTestResults)
                                                  .Include(x => x.FinalTestResults)
                                                  .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<EntityCourse?> GetIncludeCourseResult(Guid id, Guid? studentId)
        {
            return await Queryable
                         .Include(x => x.CourseResults.Where(y => y.StudentId == studentId && y.CourseId == id))
                         .Include(x => x.CourseUnitMockTests)
                         .ThenInclude(x => x.Unit)
                         .ThenInclude(x => x!.UnitResults.Where(y => y.StudentId == studentId && y.CourseId == id))
                         .Include(x => x.CourseUnitMockTests)
                         .ThenInclude(x => x.MockTest)
                         .ThenInclude(x => x!.MockTestResults.Where(y => y.StudentId == studentId && y.CourseId == id))
                         .Include(x => x.CourseUnitMockTests)
                         .ThenInclude(x => x.FinalTest)
                         .ThenInclude(x => x!.FinalTestResults.Where(y => y.StudentId == studentId && y.CourseId == id))
                         .Include(x => x.CourseTeachers)
                         .Where(x => x.Id == id)
                         .AsNoTracking()
                         .FirstOrDefaultAsync();
        }

        public async Task<(double, double, int, int)> GetContentCompleted(Guid courseId, EnumCourseType courseType, Guid? studentId)
        {
            if (courseType == EnumCourseType.Academic)
            {
                return await GetCourseAcademic(courseId, studentId);
            }
            else
            {
                return await GetCourseIELST(courseId, studentId);
            }
        }

        private async Task<(double, double, int, int)> GetCourseIELST(Guid courseId, Guid? studentId)
        {
            var course = await Queryable.Include(x => x.CourseUnitMockTests)
                                                         .ThenInclude(x => x.MockTest)
                                                         .ThenInclude(x => x!.MockTestResults.Where(x => x.StudentId == studentId && x.CourseId == courseId))
                                                         .FirstOrDefaultAsync(x => x.Id == courseId);
            var courseUnitMockTests = course?.CourseUnitMockTests.ToList();
            var unitIds = courseUnitMockTests?.Where(x => x.UnitId != null).Select(x => x.UnitId ?? default).ToList();
            var (currentProgress, progress, displayOrderUnit, displayOrderLesson) = await GetDisplayOrder(unitIds, studentId, courseUnitMockTests);
            var fullMockTest = courseUnitMockTests?.Where(x => x.MockTestId != null).Select(x => x.MockTest).ToList();
            var fullMockTestResults = fullMockTest?.SelectMany(x => x!.MockTestResults).Where(x => x.StudentId == studentId && x.CourseId == courseId).ToList();
            var count = fullMockTestResults?.Where(x => x.Status == EnumResultStatus.Done).Count() ?? default;
            return (currentProgress + count, progress + count, displayOrderUnit, displayOrderLesson);
        }

        private async Task<(double, double, int, int)> GetCourseAcademic(Guid courseId, Guid? studentId)
        {
            var course = await Queryable.Include(x => x.CourseUnitMockTests)
                                                                          .ThenInclude(x => x.FinalTest)
                                                                          .ThenInclude(x => x!.FinalTestResults.Where(x => x.StudentId == studentId && x.CourseId == courseId))
                                                                          .FirstOrDefaultAsync(x => x.Id == courseId);
            var courseUnitMockTests = course?.CourseUnitMockTests.ToList();
            var unitIds = courseUnitMockTests?.Where(x => x.UnitId != null).Select(x => x.UnitId ?? default).ToList();
            var (currentProgress, progress, displayOrderUnit, displayOrderLesson) = await GetDisplayOrder(unitIds, studentId, courseUnitMockTests);
            var finalTest = courseUnitMockTests?.Where(x => x.FinalTestId != null).Select(x => x.FinalTest).ToList();
            var finalTestResult = finalTest?.Where(x => x!.FinalTestResults.Any()).SelectMany(x => x!.FinalTestResults).FirstOrDefault(x => x.StudentId == studentId && x.CourseId == courseId);
            var count = finalTestResult?.Status == EnumResultStatus.Done ? 1 : default;
            return (currentProgress + count, progress + count, displayOrderUnit + count, displayOrderLesson);
        }

        private async Task<(double, double, int, int)> GetDisplayOrder(IList<Guid>? unitIds, Guid? studentId, IList<CourseUnitMockTest>? courseUnitMockTests)
        {
            if (unitIds == null || !unitIds.Any() || courseUnitMockTests == null || !courseUnitMockTests.Any())
            {
                return (0, 0, 0, 0);
            }
            var displayOrderLesson = 0;
            var displayOrderUnit = 0;
            var units = await _unitRepository.GetsByIds(unitIds, studentId);
            if (units != null && units.Any())
            {
                var unitResult = units.SelectMany(x => x.UnitResults).OrderByDescending(x => x.UpdatedDate).FirstOrDefault(x => x.Status != EnumResultStatus.Unfinished);
                if (unitResult != null)
                {
                    displayOrderUnit = courseUnitMockTests.FirstOrDefault(x => x.UnitId == unitResult.UnitId)?.DisplayOrder ?? default;
                    var unitContents = courseUnitMockTests.Where(x => x.DisplayOrder < displayOrderUnit && x.UnitId != null).Select(x => x.UnitId ?? default).ToList();
                    var lessonResult = await _lessonResultRepository.Queryable.OrderBy(x => x.CreatedDate).Where(x => x.UnitId == unitResult.UnitId && x.StudentId == studentId && x.Status != EnumResultStatus.Unfinished).FirstOrDefaultAsync();
                    if (lessonResult != null)
                    {
                        displayOrderLesson = units.Where(x => unitContents.Contains(x.Id)).SelectMany(x => x.UnitLessons).Where(x => unitContents.Contains(x.UnitId)).Count();
                        var a = units.Where(x => x.Id == unitResult.UnitId).SelectMany(x => x.UnitLessons).FirstOrDefault(x => x.UnitId == unitResult.UnitId && x.LessonId == lessonResult.LessonId);
                        displayOrderLesson += a?.DisplayOrder + 1 ?? default;
                    }
                }
            }
            var (currentProgress, progress) = await GetContentComplete(units, studentId);
            return (currentProgress, progress, displayOrderUnit, displayOrderLesson);
        }

        private async Task<(int, int)> GetContentComplete(IList<Unit>? units, Guid? studentId)
        {
            var countVideo = 0;
            var countHomeWork = 0;
            var countClassForum = 0;
            var lessonIds = new List<Guid>();
            if (units != null && units.Any())
            {
                lessonIds = units.SelectMany(x => x.UnitLessons).Select(x => x.Lesson).Select(x => x!.Id).ToList();
                var lessonResultIds = units.SelectMany(x => x.LessonResults).Where(x => lessonIds.Contains(x.LessonId) && x.StudentId == studentId).Select(x => x.Id).ToList();
                var lessonResults = await _lessonResultRepository.GetsByIds(lessonResultIds);
                if (lessonResults != null && lessonResults.Any())
                {
                    countVideo = lessonResults.Select(x => x.VideoResult).Where(x => x != null && x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).Count();
                    countClassForum = lessonResults.SelectMany(x => x.ClassForumResults).Where(x => x != null && x.Status == EnumClassForumResultStatus.Graded && lessonResultIds.Contains(x.LessonResultId)).Count();
                    countHomeWork = lessonResults.SelectMany(x => x.HomeWorkResults).Where(x => x != null && x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).GroupBy(x => x.LessonResultId).Count();
                }
            }
            return (countVideo + countClassForum + countHomeWork, lessonIds.Count * 3);
        }
    }
}
