// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManageProgressQuery
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
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetManageStudentProgressUnitQuery : IRequest<MethodResult<IList<ManagerUnitProgressModel>>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class GetManageStudentProgressUnitQueryHandler : IRequestHandler<GetManageStudentProgressUnitQuery, MethodResult<IList<ManagerUnitProgressModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public GetManageStudentProgressUnitQueryHandler(ICourseRepository courseRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, ILessonResultRepository lessonResultRepository, IUnitRepository unitRepository, IMockTestRepository mockTestRepository, IFinalTestRepository finalTestRepository, IUserService userService, ISystemService systemService, ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _courseRepository = courseRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _unitRepository = unitRepository;
            _mockTestRepository = mockTestRepository;
            _finalTestRepository = finalTestRepository;
            _userService = userService;
            _systemService = systemService;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
        }

        public async Task<MethodResult<IList<ManagerUnitProgressModel>>> Handle(GetManageStudentProgressUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ManagerUnitProgressModel>> methodResult = new MethodResult<IList<ManagerUnitProgressModel>>();
            IList<ManagerUnitProgressModel> managerCourseProgress = new List<ManagerUnitProgressModel>();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { request.StudentId });
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }

            var student = studentResults?.Content?.Result?.FirstOrDefault();
            var studentId = student?.Id;
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
            foreach (var courseUnit in courseUnitMockTests)
            {
                managerCourseProgress.Add(await GetManagerUnit(courseUnit, studentId, course.CourseType, userId ?? default));
            }

            methodResult.Result = default;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<ManagerUnitProgressModel> GetManagerUnit(CourseUnitMockTest courseUnitMockTest, Guid? studentId, EnumCourseType type, Guid userId)
        {
            ManagerUnitProgressModel managerUnit = new ManagerUnitProgressModel();
            if (courseUnitMockTest.UnitId != null)
            {
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
                    var featureAccessTimeResult = await _systemService.GetFeatureAccessTimesByUnitIdAsync(new GetFeatureAccessTimesByUnitIdQueryModel { CourseId = courseUnitMockTest.CourseId, UnitId = courseUnitMockTest.UnitId ?? default, UserId = userId });
                    var featureAccessTime = featureAccessTimeResult?.Content?.Result;
                    var unitResult = unit.UnitResults.FirstOrDefault(x => x.StudentId == studentId && x.UnitId == unit.Id && x.CourseId == courseUnitMockTest.CourseId);
                    managerUnit.Type = nameof(courseUnitMockTest.Unit);
                    managerUnit.ObjectId = unit.Id;
                    managerUnit.Name = unit.Name;
                    if (unitResult != null)
                    {
                        managerUnit.Status = unitResult.Status;
                        managerUnit.PercentObject = unitResult.Percent;
                        managerUnit.SkillScores = unitResult.SkillScores;
                    }

                    managerUnit.ContentProgress = string.Format("{0} / {1}", currentProgress, progress);
                    managerUnit.TotalLesson = lessonIds.Count;
                    if (featureAccessTime != null)
                    {
                        managerUnit.TimeSpent = featureAccessTime.AccessTime;
                        managerUnit.LastVisited = featureAccessTime.LastVisited;
                    }
                }
            }
            else if (type == EnumCourseType.Academic && courseUnitMockTest.FinalTestId != null)
            {
                var finalTest = await _finalTestRepository.Queryable.Include(x => x.FinalTestResults.Where(x => x.FinalTestId == courseUnitMockTest.FinalTestId && x.CourseId == courseUnitMockTest.CourseId && x.StudentId == x.StudentId))
                    .FirstOrDefaultAsync(x => x.Id == courseUnitMockTest.FinalTestId);
                if (finalTest != null)
                {
                    var finalTestResult = finalTest.FinalTestResults.FirstOrDefault(x => x.FinalTestId == courseUnitMockTest.FinalTestId && x.CourseId == courseUnitMockTest.CourseId && x.StudentId == x.StudentId);
                    managerUnit.Type = nameof(courseUnitMockTest.FinalTest);
                    managerUnit.ObjectId = finalTest.Id;
                    managerUnit.Name = finalTest.Name;
                    if (finalTestResult != null)
                    {
                        var featureAccessTimeResult = await _systemService.GetFeatureAccessTimesByTestAsync(new GetFeatureAccessTimesByTestQueryModel { CourseId = courseUnitMockTest.CourseId, ObjectId = finalTestResult.Id, UserId = userId, EnumFeature = EnumFeature.FinalTest });
                        var featureAccessTime = featureAccessTimeResult?.Content?.Result;
                        managerUnit.Status = finalTestResult.Status;
                        managerUnit.PercentObject = finalTestResult.Percent;
                        managerUnit.SkillScores = finalTestResult.SkillScores;
                        managerUnit.ContentProgress = string.Format("{0} / {1}", finalTestResult.Status == EnumResultStatus.Done ? 3 : 0, 3);
                        if (featureAccessTime != null)
                        {
                            managerUnit.TimeSpent = featureAccessTime.AccessTime;
                            managerUnit.LastVisited = featureAccessTime.LastVisited;
                        }
                    }
                }
            }
            else if (type == EnumCourseType.Ielts && courseUnitMockTest.MockTestId != null)
            {
                var mockTest = await _mockTestRepository.Queryable.Include(x => x.MockTestResults.Where(x => x.MockTestId == courseUnitMockTest.MockTestId && x.CourseId == courseUnitMockTest.CourseId && x.StudentId == x.StudentId))
                     .FirstOrDefaultAsync(x => x.Id == courseUnitMockTest.MockTestId);
                if (mockTest != null)
                {
                    var mockTestResult = mockTest.MockTestResults.FirstOrDefault(x => x.MockTestId == courseUnitMockTest.MockTestId && x.CourseId == courseUnitMockTest.CourseId && x.StudentId == x.StudentId);
                    managerUnit.Type = nameof(courseUnitMockTest.FinalTest);
                    managerUnit.ObjectId = mockTest.Id;
                    managerUnit.Name = mockTest.Name;
                    if (mockTestResult != null)
                    {
                        var featureAccessTimeResult = await _systemService.GetFeatureAccessTimesByTestAsync(new GetFeatureAccessTimesByTestQueryModel { CourseId = courseUnitMockTest.CourseId, ObjectId = mockTestResult.Id, UserId = userId, EnumFeature = EnumFeature.FinalTest });
                        var featureAccessTime = featureAccessTimeResult?.Content?.Result;
                        managerUnit.Status = mockTestResult.Status;
                        managerUnit.PercentObject = mockTestResult.Percent;
                        managerUnit.SkillScores = mockTestResult.SkillScores;
                        managerUnit.ContentProgress = string.Format("{0} / {1}", mockTestResult.Status == EnumResultStatus.Done ? 4 : 0, 4);
                        if (featureAccessTime != null)
                        {
                            managerUnit.TimeSpent = featureAccessTime.AccessTime;
                            managerUnit.LastVisited = featureAccessTime.LastVisited;
                        }
                    }
                }
            }
            return managerUnit;
        }

        private async Task<(int, int)> GetContentComplete(IList<Guid>? lessonIds, Guid? studentId, Guid? mockTestId)
        {
            var countVideo = 0;
            var countHomeWork = 0;
            var countClassForum = 0;
            var countMockTest = 0;
            if (lessonIds != null && lessonIds.Any())
            {
                var lessonResults = await _lessonResultRepository.GetsByLessonIds(lessonIds, studentId);
                if (lessonResults != null && lessonResults.Any())
                {
                    countVideo = lessonResults.Select(x => x.VideoResult).Where(x => x != null && x.Status == EnumResultStatus.Done && x.StudentId == studentId).Count();
                    countClassForum = lessonResults.SelectMany(x => x.ClassForumResults).Where(x => x != null && x.Status == EnumClassForumResultStatus.Graded && x.StudentId == studentId).Count();
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
