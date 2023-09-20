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

    public class GetStudentManageLessonQuery : IRequest<MethodResult<IList<LessonManagerProgressModel>>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
    }

    public class GetStudentManageLessonQueryHandler : IRequestHandler<GetStudentManageLessonQuery, MethodResult<IList<LessonManagerProgressModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;

        public GetStudentManageLessonQueryHandler(ICourseRepository courseRepository, IMockTestResultRepository mockTestResultRepository, IMockTestRepository mockTestRepository, ILessonResultRepository lessonResultRepository, IUnitRepository unitRepository, IUserService userService, ISystemService systemService)
        {
            _courseRepository = courseRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestRepository = mockTestRepository;
            _lessonResultRepository = lessonResultRepository;
            _unitRepository = unitRepository;
            _userService = userService;
            _systemService = systemService;
        }

        public async Task<MethodResult<IList<LessonManagerProgressModel>>> Handle(GetStudentManageLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<LessonManagerProgressModel>> methodResult = new MethodResult<IList<LessonManagerProgressModel>>();
            IList<LessonManagerProgressModel> managerCourseProgress = new List<LessonManagerProgressModel>();
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
            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesByLessonIdsAsync(new FeatureAccessTimesByLessonIdsQueryModel { CourseId = request.CourseId, UnitId = request.UnitId, LessonIds = lessonIds, UserId = userId ?? default });
            if (!featureAccessTimeResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResults));
                return methodResult;
            }
            var featureAccessTimes = featureAccessTimeResults.Content?.Result;
            foreach (var item in lessonIds)
            {
                var managerUnit = await GetManagerLesson(item, studentId);
                var featureAccessTime = featureAccessTimes?.FirstOrDefault(x => x.LessonId == item);
                if (featureAccessTime != null)
                {
                    managerUnit.TimeSpent = featureAccessTime.AccessTime;
                    managerUnit.LastVisited = featureAccessTime.LastVisited;
                    managerUnit.Visit = featureAccessTime.Visit;
                }
                managerCourseProgress.Add(managerUnit);
            }
            if (unit.UnitSkillMockTests.Any())
            {
                var mockTestId = unit.UnitSkillMockTests.Select(x => x.MockTestId).FirstOrDefault();
                var mockTestResult = await _mockTestResultRepository.Queryable.Where(x => x.MockTestId == mockTestId && x.StudentId == studentId).FirstOrDefaultAsync(cancellationToken);
                var mockTestResults = await _systemService.GetFeatureAccessTimesBySkillMockTestAsync(new FeatureAccessTimesByMockTestIdQueryModel { CourseId = request.CourseId, ObjectId = mockTestId, UnitId = request.UnitId, UserId = userId ?? default });
                var featureAccessTimeTest = mockTestResults?.Content?.Result;
                managerCourseProgress.Add(await GetMockTestManager(request, mockTestId, featureAccessTimeTest));
            }

            methodResult.Result = managerCourseProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<LessonManagerProgressModel> GetManagerLesson(Guid? lessonId, Guid? studentId)
        {
            LessonManagerProgressModel managerUnit = new LessonManagerProgressModel();
            var countVideo = 0;
            var countHomeWork = 0;
            var countClassForum = 0;
            var lessonResult = await _lessonResultRepository.GetByLessonId(lessonId, studentId);
            if (lessonResult != null)
            {
                var lesson = lessonResult.Lesson;
                countVideo = lessonResult.VideoResult?.Status == EnumResultStatus.Done ? 1 : 0;
                countClassForum = lessonResult.ClassForumResults.Where(x => x != null && x.Status == EnumClassForumResultStatus.Graded && x.StudentId == studentId).Count();
                countHomeWork = lessonResult.HomeWorkResults.Where(x => x != null && x.Status == EnumResultStatus.Done && x.StudentId == studentId).GroupBy(x => x.LessonResultId).Count();
                var list = new List<int> { countHomeWork, countClassForum, countVideo };
                if (lesson != null)
                {
                    managerUnit.ObjectId = lesson.Id;
                    managerUnit.Name = lesson.Name;
                }
                managerUnit.Status = lessonResult.Status;
                managerUnit.Percent = NumberHelper.ConvertPercentDouble(list.Average());
                managerUnit.ContentCompleted = string.Format("{0} / {1}", list.Sum(), 3);
            }
            return managerUnit;
        }

        private async Task<LessonManagerProgressModel> GetMockTestManager(GetStudentManageLessonQuery request, Guid mockTestId, FeatureAccessTimeCourseModel? featureAccessTime)
        {
            LessonManagerProgressModel managerUnit = new LessonManagerProgressModel();
            var mockTest = await _mockTestRepository.Queryable.Include(x => x.MockTestResults.Where(x => x.MockTestId == mockTestId && x.CourseId == request.CourseId && x.StudentId == request.StudentId && x.UnitId == request.UnitId))
                        .FirstOrDefaultAsync(x => x.Id == mockTestId);
            if (mockTest != null)
            {
                var mockTestResult = mockTest.MockTestResults.FirstOrDefault(x => x.MockTestId == mockTestId && x.CourseId == request.CourseId && x.StudentId == request.StudentId && x.UnitId == request.UnitId);
                managerUnit.Type = nameof(mockTestResult.MockTest);
                managerUnit.ObjectId = mockTest.Id;
                managerUnit.Name = mockTest.Name;
                if (mockTestResult != null)
                {
                    managerUnit.Status = mockTestResult.Status;
                    managerUnit.PercentObject = mockTestResult.Percent;
                    managerUnit.SkillScores = mockTestResult.SkillScores;
                    var isDone = mockTestResult.Status == EnumResultStatus.Done;
                    managerUnit.ContentCompleted = string.Format("{0} / {1}", isDone ? 1 : 0, 1);
                    managerUnit.Percent = isDone ? 100 : 0;
                    if (featureAccessTime != null)
                    {
                        managerUnit.TimeSpent = featureAccessTime.AccessTime;
                        managerUnit.LastVisited = featureAccessTime.LastVisited;
                    }
                }
            }
            return managerUnit;
        }
    }
}
