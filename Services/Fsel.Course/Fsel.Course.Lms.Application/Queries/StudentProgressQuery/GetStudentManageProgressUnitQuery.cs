// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using System.Linq.Dynamic.Core;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentManageProgressUnitQuery : IRequest<MethodResult<UnitManagerProgressModel>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
    }

    public class GetStudentManageProgressUnitQueryHandler : IRequestHandler<GetStudentManageProgressUnitQuery, MethodResult<UnitManagerProgressModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;

        public GetStudentManageProgressUnitQueryHandler(ICourseRepository courseRepository, IMockTestResultRepository mockTestResultRepository, ILessonResultRepository lessonResultRepository, IUnitRepository unitRepository, IUserService userService, ISystemService systemService)
        {
            _courseRepository = courseRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _unitRepository = unitRepository;
            _userService = userService;
            _systemService = systemService;
        }

        public async Task<MethodResult<UnitManagerProgressModel>> Handle(GetStudentManageProgressUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UnitManagerProgressModel> methodResult = new MethodResult<UnitManagerProgressModel>();
            UnitManagerProgressModel managerCourseProgress = new UnitManagerProgressModel();
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
            var unit = await _unitRepository.Queryable.Include(x => x.UnitResults.Where(x => x.StudentId == studentId && x.UnitId == request.UnitId))
                                                         .Include(x => x.UnitLessons)
                                                         .Include(x => x.UnitSkillMockTests)
                                                         .FirstOrDefaultAsync(x => x.Id == request.UnitId, cancellationToken);
            if (unit != null)
            {
                var lessonIds = unit.UnitLessons.Select(x => x.LessonId).ToList();
                var mockTestId = unit.UnitSkillMockTests.Any() ? unit.UnitSkillMockTests.FirstOrDefault()?.Id : null;
                var (currentProgress, progress) = await GetContentComplete(lessonIds, studentId, mockTestId);
                var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesByUnitIdAsync(new FeatureAccessTimesByUnitIdQueryModel { CourseId = request.CourseId, UnitIds = new List<Guid> { request.UnitId }, UserId = userId ?? default });
                var featureAccessTimes = featureAccessTimeResults?.Content?.Result;
                var unitResult = unit.UnitResults.FirstOrDefault(x => x.StudentId == studentId && x.UnitId == unit.Id && x.CourseId == request.CourseId);
                managerCourseProgress.Type = nameof(unitResult.Unit);
                managerCourseProgress.ObjectId = unit.Id;
                managerCourseProgress.Name = unit.Name;
                if (unitResult != null)
                {
                    managerCourseProgress.Status = unitResult.Status;
                    managerCourseProgress.PercentObject = unitResult.Percent;
                    managerCourseProgress.SkillScores = unitResult.SkillScores;
                }

                managerCourseProgress.ContentProgress = string.Format("{0} / {1}", currentProgress, progress);
                managerCourseProgress.TotalLesson = lessonIds.Count;
                if (featureAccessTimes != null && featureAccessTimes.Any())
                {
                    managerCourseProgress.TimeSpent = featureAccessTimes.FirstOrDefault()!.AccessTime;
                    managerCourseProgress.LastVisited = featureAccessTimes.FirstOrDefault()!.LastVisited;
                }
            }
            methodResult.Result = managerCourseProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
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
