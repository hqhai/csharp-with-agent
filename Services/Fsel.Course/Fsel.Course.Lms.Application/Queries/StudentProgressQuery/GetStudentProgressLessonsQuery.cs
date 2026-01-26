// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using System.Collections.Generic;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;

    public class GetStudentProgressLessonsQuery : IRequest<MethodResult<IList<LessonStudentProgressModel>>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid? ModuleId { get; set; }
    }

    public class GetStudentProgressLessonsQueryHandler : IRequestHandler<GetStudentProgressLessonsQuery, MethodResult<IList<LessonStudentProgressModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IMockTestSectionRepository _mockTestSectionRepository;
        private readonly IMockTestScoreRepository _mockTestScoreRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private const int MaxModuleLesson = 3;
        private readonly ILearningService _learningService;

        public GetStudentProgressLessonsQueryHandler(ICourseRepository courseRepository, ManagerProgressHelper managerProgressHelper, ISectionGroupRepository sectionGroupRepository, IMockTestSectionRepository mockTestSectionRepository, IMockTestScoreRepository mockTestScoreRepository, IMockTestResultRepository mockTestResultRepository, IMockTestRepository mockTestRepository, ILessonResultRepository lessonResultRepository, IUnitRepository unitRepository, IUserService userService, ISystemService systemService, ILearningService learningService)
        {
            _courseRepository = courseRepository;
            _managerProgressHelper = managerProgressHelper;
            _sectionGroupRepository = sectionGroupRepository;
            _mockTestSectionRepository = mockTestSectionRepository;
            _mockTestScoreRepository = mockTestScoreRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestRepository = mockTestRepository;
            _lessonResultRepository = lessonResultRepository;
            _unitRepository = unitRepository;
            _userService = userService;
            _systemService = systemService;
            _learningService = learningService;
        }

        public async Task<MethodResult<IList<LessonStudentProgressModel>>> Handle(GetStudentProgressLessonsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LessonStudentProgressModel>>();

            var studentResults = await _userService.GetUserByStudentIdWithCache(request.StudentId);
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }
            var student = studentResults?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student.Id;
            var userId = student.UserId;

            var learningTree = await _learningService.GetLearningTreeFromCourseToTest(
                  student.Id,
                  request.CourseId,
                  request.ModuleId,
                  cancellationToken);

            if (learningTree == null)
            {
                return methodResult;
            }

            var units = learningTree
               .GetAllItemByType<UnitComponentModel>().OrderBy(p => p.DisplayOrder)
               .ToList();

            var unit = units.FirstOrDefault(p => p.LearningTemplateId == request.UnitId);

            if (unit == null || unit.Children == null)
            {
                return methodResult;
            }

            var lessonIds = unit.Children.Select(p => p.LearningTemplateId).ToList();

            var featureAccessTimeModels = lessonIds.Select(x => new FeatureAccessTimeQueryModel
            {
                CourseId = request.CourseId,
                UnitId = request.UnitId,
                LessonId = x,
                UserId = userId
            }).ToList();

            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesAsync(new FeatureAccessTimesQueryModel
            {
                FeatureAccessTimes = featureAccessTimeModels,
                UserId = userId
            });
            if (!featureAccessTimeResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResults));
                return methodResult;
            }
            var featureAccessTimes = featureAccessTimeResults.Content?.Result;

            var listLessonProgress = new List<LessonStudentProgressModel>();

            foreach (var item in unit.Children)
            {
                var lessonProgress = new LessonStudentProgressModel()
                {
                    Type = item.Type,
                    ObjectId = item.LearningTemplateId,
                    Id = item.Id,
                    Name = item.ComponentName,
                    Status = item.Status ?? EnumResultStatus.Unfinished,
                    ContentCompleted = $"{item.TotalContentCompleted} / {item.TotalContent}",
                    DisplayOrder = item.DisplayOrder,
                    LearningResultId = item.LearningResultId
                };

                if (item.Type == EnumUnitConfigType.Lesson.ToString())
                {
                    var featureAccessTime = featureAccessTimes?.FirstOrDefault(x => x.LessonId == item.LearningTemplateId);

                    lessonProgress.Percent = NumberHelper.GetPercent(item.TotalContentCompleted, item.TotalContent);

                    if (featureAccessTime != null)
                    {
                        lessonProgress.TimeSpent = featureAccessTime.AccessTime;
                        lessonProgress.LastVisited = featureAccessTime.LastVisited ?? null;
                        lessonProgress.Visit = featureAccessTime.Visit;
                    }
                }
                else
                {
                    var featureAccessTimeTestResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel
                    {
                        CourseId = request.CourseId,
                        UnitId = request.UnitId,
                        ObjectId = item.LearningResultId,
                        UserId = userId,
                        EnumFeature = EnumFeature.MockTest
                    });
                    var featureAccessTimeTest = featureAccessTimeTestResult?.Content?.Result;

                    lessonProgress.Percent = lessonProgress.Status == EnumResultStatus.Done ? 100 : 0;
                    lessonProgress.Score = item.Score;

                    lessonProgress.TimeSpent = featureAccessTimeTest?.AccessTime ?? 0;
                    lessonProgress.LastVisited = featureAccessTimeTest?.LastVisited ?? null;
                    lessonProgress.Visit = featureAccessTimeTest?.Visit ?? 0;
                }

                listLessonProgress.Add(lessonProgress);
            }

            methodResult.Result = listLessonProgress.OrderBy(p => p.DisplayOrder).ToList();
            return methodResult;
        }
    }
}
