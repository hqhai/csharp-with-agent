// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using System.Collections.Generic;
    using System.Linq.Dynamic.Core;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProgressLessonsQuery : IRequest<MethodResult<IList<LessonStudentProgressModel>>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
    }

    public class GetStudentProgressLessonsQueryHandler : IRequestHandler<GetStudentProgressLessonsQuery, MethodResult<IList<LessonStudentProgressModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;

        public GetStudentProgressLessonsQueryHandler(ICourseRepository courseRepository, IMockTestResultRepository mockTestResultRepository, IMockTestRepository mockTestRepository, ILessonResultRepository lessonResultRepository, IUnitRepository unitRepository, IUserService userService, ISystemService systemService)
        {
            _courseRepository = courseRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestRepository = mockTestRepository;
            _lessonResultRepository = lessonResultRepository;
            _unitRepository = unitRepository;
            _userService = userService;
            _systemService = systemService;
        }

        public async Task<MethodResult<IList<LessonStudentProgressModel>>> Handle(GetStudentProgressLessonsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<LessonStudentProgressModel>> methodResult = new MethodResult<IList<LessonStudentProgressModel>>();
            IList<LessonStudentProgressModel> listLessonProgress = new List<LessonStudentProgressModel>();
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
            var unit = new Domain.Entities.Unit();
            if (course.CourseType == EnumCourseType.Academic)
            {
                unit = await _unitRepository.Queryable.Include(x => x.UnitLessons).FirstOrDefaultAsync(x => x.Id == request.UnitId, cancellationToken);
            }
            else
            {
                unit = await _unitRepository.Queryable.Include(x => x.UnitLessons).Include(x => x.UnitSkillMockTests).FirstOrDefaultAsync(x => x.Id == request.UnitId, cancellationToken);
            }
            if (unit == null)
            {
                methodResult.Result = default;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var lessonIds = unit.UnitLessons.OrderBy(x => x.CreatedDate).Select(x => x.LessonId).ToList();
            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesAsync(new FeatureAccessTimesQueryModel
            {
                FeatureAccessTimes = lessonIds.Select(x => new FeatureAccessTimeQueryModel
                {
                    CourseId = request.CourseId,
                    UnitId = request.UnitId,
                    LessonId = x,
                    UserId = userId ?? default
                }).ToList(),
                UserId = userId ?? default
            });
            if (!featureAccessTimeResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResults));
                return methodResult;
            }
            var featureAccessTimes = featureAccessTimeResults.Content?.Result;
            foreach (var item in lessonIds)
            {
                var managerUnit = await GetLesson(item, studentId);
                var featureAccessTime = featureAccessTimes?.FirstOrDefault(x => x.LessonId == item);
                managerUnit.Type = nameof(managerUnit.Type);
                if (featureAccessTime != null)
                {
                    managerUnit.TimeSpent = featureAccessTime.AccessTime;
                    managerUnit.LastVisited = featureAccessTime.LastVisited ?? default;
                    managerUnit.Visit = featureAccessTime.Visit;
                }
                listLessonProgress.Add(managerUnit);
            }
            if (unit.UnitSkillMockTests.Any())
            {
                var mockTestId = unit.UnitSkillMockTests.Select(x => x.MockTestId).FirstOrDefault();
                var mockTestResult = await _mockTestResultRepository.Queryable.Where(x => x.MockTestId == mockTestId && x.StudentId == studentId).FirstOrDefaultAsync(cancellationToken);
                var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel { CourseId = request.CourseId, ObjectId = mockTestId, UserId = userId ?? default, EnumFeature = EnumFeature.MockTest });
                var featureAccessTimeTest = featureAccessTimeResult?.Content?.Result;
                listLessonProgress.Add(await GetMockTest(request, mockTestId, featureAccessTimeTest));
            }

            methodResult.Result = listLessonProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<LessonStudentProgressModel> GetLesson(Guid? lessonId, Guid? studentId)
        {
            LessonStudentProgressModel lessonProgress = new LessonStudentProgressModel();
            var counts = new List<int>();
            var lessonResult = await _lessonResultRepository.GetAsync(lessonId, studentId);
            lessonProgress.Type = nameof(lessonResult.Lesson);
            if (lessonResult != null)
            {
                var lesson = lessonResult.Lesson;
                counts.Add(lessonResult.VideoResult?.Status == EnumResultStatus.Done ? 1 : 0);
                counts.Add(lessonResult.ClassForumResults.Where(x => x != null && x.Status == EnumClassForumResultStatus.Graded && x.StudentId == studentId).Count());
                counts.Add(lessonResult.HomeWorkResults.Where(x => x != null && x.Status == EnumResultStatus.Done && x.StudentId == studentId).GroupBy(x => x.LessonResultId).Count());
                if (lesson != null)
                {
                    lessonProgress.ObjectId = lesson.Id;
                    lessonProgress.Name = lesson.Name;
                }
                lessonProgress.Status = lessonResult.Status;
                lessonProgress.Percent = NumberHelper.ConvertPercentDouble(counts.Average());
                lessonProgress.ContentCompleted = string.Format("{0} / {1}", counts.Sum(), 3);
            }
            return lessonProgress;
        }

        private async Task<LessonStudentProgressModel> GetMockTest(GetStudentProgressLessonsQuery request, Guid mockTestId, FeatureAccessTimeModel? featureAccessTime)
        {
            LessonStudentProgressModel mockTestProgress = new LessonStudentProgressModel();
            var mockTest = await _mockTestRepository.Queryable.Include(x => x.MockTestResults.Where(x => x.MockTestId == mockTestId && x.CourseId == request.CourseId && x.StudentId == request.StudentId && x.UnitId == request.UnitId))
                        .FirstOrDefaultAsync(x => x.Id == mockTestId);
            if (mockTest != null)
            {
                var mockTestResult = mockTest.MockTestResults.FirstOrDefault(x => x.MockTestId == mockTestId && x.CourseId == request.CourseId && x.StudentId == request.StudentId && x.UnitId == request.UnitId);
                mockTestProgress.Type = nameof(mockTestResult.MockTest);
                mockTestProgress.ObjectId = mockTest.Id;
                mockTestProgress.Name = mockTest.Name;
                if (mockTestResult != null)
                {
                    mockTestProgress.Status = mockTestResult.Status;
                    mockTestProgress.PercentObject = mockTestResult.Percent;
                    mockTestProgress.SkillScores = mockTestResult.SkillScores;
                    var isDone = mockTestResult.Status == EnumResultStatus.Done;
                    mockTestProgress.ContentCompleted = string.Format("{0} / {1}", isDone ? 1 : 0, 1);
                    mockTestProgress.Percent = isDone ? 100 : 0;
                    if (featureAccessTime != null)
                    {
                        mockTestProgress.TimeSpent = featureAccessTime.AccessTime;
                        mockTestProgress.LastVisited = featureAccessTime.LastVisited ?? default;
                    }
                }
            }
            return mockTestProgress;
        }
    }
}
