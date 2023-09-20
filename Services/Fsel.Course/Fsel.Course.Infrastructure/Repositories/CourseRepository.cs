// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using EntityCourse = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class CourseRepository : BaseRepository<EntityCourse>, ICourseRepository
    {
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public CourseRepository(CourseDbContext dbContext, IUnitResultRepository unitResultRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, AuthContext authContext, IUnitRepository unitRepository, ILessonResultRepository lessonResultRepository) : base(dbContext, authContext)
        {
            _unitResultRepository = unitResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
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

        public async Task<(int, int)> GetDisplayOrder(CourseResultModel courseResult)
        {
            ArgumentNullException.ThrowIfNull(courseResult);

            var displayOrderLesson = 0;
            var displayOrderUnit = 0;
            var unitResult = await _unitResultRepository.Queryable.Include(x => x.Unit).ThenInclude(x => x!.CourseUnitMockTests).Where(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId && x.Status != EnumResultStatus.Unfinished).OrderBy(x => x.CreatedDate).ThenBy(x => x.UpdatedDate).FirstOrDefaultAsync();
            if (unitResult != null)
            {
                displayOrderUnit = unitResult.Unit?.CourseUnitMockTests.FirstOrDefault()?.DisplayOrder ?? default;
                var lessonResults = await _lessonResultRepository.Queryable.Where(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId).ToListAsync();
                if (lessonResults != null && lessonResults.Any())
                {
                    if (lessonResults.All(x => x.Status == EnumResultStatus.Done))
                    {
                        displayOrderLesson = lessonResults.Count;
                    }
                    else
                    {
                        displayOrderLesson = lessonResults.Where(x => x.Status == EnumResultStatus.Done).Count() + 1;
                    }
                }
            }
            return (displayOrderUnit, displayOrderLesson);
        }

        public async Task<(int, int)> GetContentComplete(CourseResultModel courseResult)
        {
            ArgumentNullException.ThrowIfNull(courseResult);
            var counts = new List<int>();
            var countTests = new List<(int, int)>();
            var lessonResults = await _lessonResultRepository.GetByCourseResult(courseResult);
            if (lessonResults != null && lessonResults.Any())
            {
                var lessonResultIds = lessonResults.Select(x => x.Id).ToList();
                counts.Add(lessonResults.Select(x => x.VideoResult).Where(x => x != null && x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).Count());
                counts.Add(lessonResults.SelectMany(x => x.ClassForumResults).Where(x => x != null && x.Status == EnumClassForumResultStatus.Graded && lessonResultIds.Contains(x.LessonResultId)).Count());
                counts.Add(lessonResults.SelectMany(x => x.HomeWorkResults).Where(x => x != null && x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).GroupBy(x => x.LessonResultId).Count());
            }
            if (courseResult.CourseType == EnumCourseType.Academic)
            {
                var finalTestResult = await _finalTestResultRepository.Queryable.Where(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId).FirstOrDefaultAsync();
                countTests.Add((finalTestResult?.Status == EnumResultStatus.Done ? 1 : 0, 1));
            }
            else
            {
                var mockTestResults = await _mockTestResultRepository.Queryable.Where(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId).ToListAsync();
                countTests.Add((mockTestResults.Where(x => x.Status == EnumResultStatus.Done).Count(), mockTestResults?.Count ?? default));
            }
            return (counts.Sum() + countTests.Sum(x => x.Item1), (lessonResults?.Count ?? default) * 3 + countTests.Sum(x => x.Item2));
        }
    }
}
