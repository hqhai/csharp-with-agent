// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using System.Linq.Dynamic.Core;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProgressUnitsQuery : IRequest<MethodResult<IList<UnitStudentProgressModel>>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class GetStudentProgressUnitsQueryHandler : IRequestHandler<GetStudentProgressUnitsQuery, MethodResult<IList<UnitStudentProgressModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public GetStudentProgressUnitsQueryHandler(ICourseRepository courseRepository, IMockTestResultRepository mockTestResultRepository, ILessonResultRepository lessonResultRepository, IUnitRepository unitRepository, IUserService userService, ISystemService systemService, ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _courseRepository = courseRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _unitRepository = unitRepository;
            _userService = userService;
            _systemService = systemService;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
        }

        public async Task<MethodResult<IList<UnitStudentProgressModel>>> Handle(GetStudentProgressUnitsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<UnitStudentProgressModel>> methodResult = new MethodResult<IList<UnitStudentProgressModel>>();
            IList<UnitStudentProgressModel> unitStudentProgress = new List<UnitStudentProgressModel>();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { request.StudentId });
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }

            var student = studentResults?.Content?.Result?.FirstOrDefault();
            var userId = student?.Human?.UserId;
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.Result = default;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var courseUnitMockTests = await _courseUnitMockTestRepository.Queryable.Where(x => x.CourseId == request.CourseId).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);
            if (courseUnitMockTests == null || !courseUnitMockTests.Any())
            {
                methodResult.Result = default;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var featureAccessTimeTest = new List<FeatureAccessTimeModel>();
            var unitIds = courseUnitMockTests.Where(x => x.UnitId != null).Select(x => x.UnitId ?? default).ToList();
            var unitResults = await _systemService.GetFeatureAccessTimesAsync(new FeatureAccessTimesQueryModel
            {
                UserId = userId ?? default,
                FeatureAccessTimes = unitIds.Select(x => new FeatureAccessTimeQueryModel
                {
                    UserId = userId ?? default,
                    UnitId = x,
                    CourseId = course.Id
                }).ToList(),
            });
            var featureAccessTimeUnit = unitResults?.Content?.Result;
            foreach (var courseUnit in courseUnitMockTests)
            {
                if (courseUnit.UnitId != null)
                {
                    UnitStudentProgressModel unitProgress = new UnitStudentProgressModel();
                    unitProgress = await GetUnitManager(courseUnit, request.StudentId, featureAccessTimeUnit);
                    unitProgress.Type = nameof(courseUnit.Unit);
                    unitStudentProgress.Add(unitProgress);
                }
            }

            methodResult.Result = unitStudentProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<UnitStudentProgressModel> GetUnitManager(CourseUnitMockTest courseUnitMockTest, Guid? studentId, IList<FeatureAccessTimeModel>? featureAccessTimeResults)
        {
            UnitStudentProgressModel unitProgress = new UnitStudentProgressModel();
            var unitId = courseUnitMockTest.UnitId;
            var unit = await _unitRepository.Queryable.Include(x => x.UnitResults.Where(x => x.StudentId == studentId && x.UnitId == unitId))
                                                    .Include(x => x.UnitLessons)
                                                    .Include(x => x.UnitSkillMockTests)
                                                    .FirstOrDefaultAsync(x => x.Id == unitId);
            if (unit != null)
            {
                var lessonIds = unit.UnitLessons.Select(x => x.LessonId).ToList();
                var mockTestId = unit.UnitSkillMockTests.Any() ? unit.UnitSkillMockTests.FirstOrDefault()?.Id : null;
                var (currentProgress, progress) = await GetContentComplete(lessonIds, studentId, mockTestId);
                var featureAccessTime = featureAccessTimeResults?.FirstOrDefault(x => x.UnitId == unitId);
                var unitResult = unit.UnitResults.FirstOrDefault(x => x.StudentId == studentId && x.UnitId == unit.Id && x.CourseId == courseUnitMockTest.CourseId);
                unitProgress.Type = nameof(courseUnitMockTest.Unit);
                unitProgress.ObjectId = unit.Id;
                unitProgress.Name = unit.Name;
                unitProgress.DisplayOrder = courseUnitMockTest.DisplayOrder;
                if (unitResult != null)
                {
                    unitProgress.Status = unitResult.Status;
                    unitProgress.CorrectPercent = unitResult.Percent;
                }

                unitProgress.ContentProgress = string.Format("{0} / {1}", currentProgress, progress);
                unitProgress.TotalLesson = lessonIds.Count;
                unitProgress.ProcessPercent = NumberHelper.ConvertPercentDouble((double)currentProgress / progress);
                if (featureAccessTime != null)
                {
                    unitProgress.TimeSpent = featureAccessTime.AccessTime;
                    unitProgress.LastVisited = featureAccessTime.LastVisited ?? null;
                }
            }
            return unitProgress;
        }

        private async Task<(int, int)> GetContentComplete(IList<Guid>? lessonIds, Guid? studentId, Guid? mockTestId)
        {
            var countVideo = 0;
            var countHomeWork = 0;
            var countClassForum = 0;
            var countMockTest = 0;
            if (lessonIds != null && lessonIds.Any())
            {
                var lessonResults = await _lessonResultRepository.GetListAsync(lessonIds, studentId);
                if (lessonResults != null && lessonResults.Any())
                {
                    countVideo = lessonResults.Select(x => x.VideoResult).Where(x => x != null && x.Status == EnumResultStatus.Done && x.StudentId == studentId).Count();
                    countClassForum = lessonResults.SelectMany(x => x.ClassForumResults).Where(x => x != null && (x.Status == EnumClassForumResultStatus.PendingForGrading || x.Status == EnumClassForumResultStatus.Graded) && x.StudentId == studentId).Count();
                    countHomeWork = lessonResults.SelectMany(x => x.HomeWorkResults).Where(x => x != null && x.Status == EnumResultStatus.Done && x.StudentId == studentId).GroupBy(x => x.LessonResultId).Count();
                }
            }
            if (mockTestId != null)
            {
                var mockTestResult = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestId == mockTestId && x.StudentId == studentId);
                if (mockTestResult != null)
                {
                    countMockTest = mockTestResult.Status == EnumResultStatus.Done ? 1 : 0;
                }
            }
            return (countVideo + countClassForum + countHomeWork + countMockTest, (lessonIds?.Count ?? default) * 3 + countMockTest);
        }
    }
}
