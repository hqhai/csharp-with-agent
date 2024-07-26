// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public class ManagerProgressHelper
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;

        public ManagerProgressHelper(ILessonResultRepository lessonResultRepository, ICourseRepository courseRepository, ICourseResultRepository courseResultRepository, IUnitRepository unitRepository, IMockTestResultRepository mockTestResultRepository)
        {
            _lessonResultRepository = lessonResultRepository;
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _unitRepository = unitRepository;
            _mockTestResultRepository = mockTestResultRepository;
        }

        public async Task<UnitStudentProgressModel?> GetUnitManager(Guid courseId, Guid unitId, Guid? studentId)
        {
            var unit = await _unitRepository.Queryable.Include(x => x.UnitResults.Where(x => x.StudentId == studentId && x.UnitId == unitId && x.CourseId == courseId))
                                                    .Include(x => x.UnitLessons)
                                                    .Include(x => x.UnitSkillMockTests)
                                                    .Include(x => x.CourseUnitMockTests.Where(x => x.CourseId == courseId))
                                                    .FirstOrDefaultAsync(x => x.Id == unitId);
            if (unit == null)
            {
                return default;
            }
            var courseUnitMockTest = unit.CourseUnitMockTests.FirstOrDefault();
            var lessonIds = unit.UnitLessons.Select(x => x.LessonId).ToList();
            var mockTestId = unit.UnitSkillMockTests.Any() ? unit.UnitSkillMockTests.FirstOrDefault()?.MockTestId : null;

            UnitStudentProgressModel unitProgress = new UnitStudentProgressModel();
            var (currentProgress, progress) = await GetContentComplete(lessonIds, courseId, unit.Id, studentId, mockTestId);
            var unitResult = unit.UnitResults.FirstOrDefault(x => x.StudentId == studentId && x.UnitId == unit.Id && x.CourseId == courseId);
            unitProgress.Type = nameof(CourseUnitMockTest.Unit);
            unitProgress.ObjectId = unit.Id;
            unitProgress.Name = unit.Name;
            unitProgress.DisplayOrder = courseUnitMockTest?.DisplayOrder ?? default;
            if (unitResult != null)
            {
                unitProgress.Status = unitResult.Status;
                unitProgress.CorrectPercent = unitResult.Percent;
            }
            unitProgress.ContentProgress = string.Format("{0} / {1}", currentProgress, progress);
            unitProgress.TotalLesson = lessonIds.Count;
            unitProgress.ProcessPercent = NumberHelper.GetPercent(currentProgress, progress);
            return unitProgress;
        }

        public async Task<CourseStudentProgressModel?> GetCourseManagerAsync(Guid courseId, Guid? studentId, CancellationToken cancellationToken)
        {
            var courseResult = await _courseResultRepository.Queryable.Include(x => x.Course).FirstOrDefaultAsync(x => x.StudentId == studentId && x.CourseId == courseId, cancellationToken);
            if (courseResult == null)
            {
                return default;
            }
            var courseResultModel = new CourseResultModel
            {
                CourseType = courseResult.Course?.CourseType,
                CourseId = courseResult.CourseId,
                StudentId = courseResult.StudentId
            };
            var (currentProgress, progress) = await _courseRepository.GetContentComplete(courseResultModel);
            var courseProgress = new CourseStudentProgressModel
            {
                ContentCompleted = string.Format("{0} / {1}", currentProgress, progress),
                StartDate = courseResult.ProcessDate,
                EndDate = courseResult.CompletionDate,
                CourseName = courseResult.Course?.Code,
                CourseId = courseResult.Course?.Id ?? default,
            };
            return courseProgress;
        }

        public async Task<(int, int)> GetContentComplete(IList<Guid>? lessonIds, Guid courseId, Guid unitId, Guid? studentId, Guid? mockTestId)
        {
            var counts = new List<int>();
            if (lessonIds != null && lessonIds.Any())
            {
                var lessonResults = await _lessonResultRepository.GetListAsync(lessonIds, courseId, unitId, studentId);
                if (lessonResults != null && lessonResults.Any())
                {
                    var countVideo = lessonResults.Select(x => x.VideoResult).Where(x => x != null && x.Status == EnumResultStatus.Done && x.StudentId == studentId).Count();
                    var countClassForum = lessonResults.SelectMany(x => x.ClassForumResults).Where(x => x != null && (x.Status == EnumClassForumResultStatus.Denied || x.Status == EnumClassForumResultStatus.Graded) && x.StudentId == studentId).Count();
                    var countHomeWork = lessonResults.Select(x =>
                    {
                        return x.HomeWorkResults.Any() && x.HomeWorkResults.All(x => x != null && x.Status == EnumResultStatus.Done && x.StudentId == studentId) ? 1 : 0;
                    }).Sum();
                    counts.AddRange(new List<int> { countHomeWork, countClassForum, countVideo });
                }
            }
            if (mockTestId != null)
            {
                var mockTestResult = await _mockTestResultRepository.Queryable.Where(x => x.MockTestId == mockTestId && x.UnitId == unitId)
                                                                              .FirstOrDefaultAsync(x => x.StudentId == studentId);
                if (mockTestResult != null)
                {
                    counts.Add(mockTestResult.Status == EnumResultStatus.Done ? 1 : 0);
                }
            }
            return (counts.Sum(), (lessonIds?.Count ?? default) * 3 + (mockTestId != null ? 1 : 0));
        }
    }
}
