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
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;

        public GetStudentManageLessonQueryHandler(ICourseRepository courseRepository, IMockTestResultRepository mockTestResultRepository, ILessonResultRepository lessonResultRepository, IUnitRepository unitRepository, IUserService userService, ISystemService systemService)
        {
            _courseRepository = courseRepository;
            _mockTestResultRepository = mockTestResultRepository;
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
            var lessonIds = unit.UnitLessons.Select(x => x.LessonId).ToList();
            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesByLessonIdsAsync(new FeatureAccessTimesByLessonIdsQueryModel { CourseId = request.CourseId, UnitId = request.UnitId, LessonIds = lessonIds, UserId = userId ?? default });
            if (!featureAccessTimeResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResults));
                return methodResult;
            }
            var featureAccessTimes = featureAccessTimeResults.Content?.Result;
            foreach (var item in lessonIds)
            {
                var managerUnit = await GetManagerLesson(item, studentId, course.CourseType, userId ?? default);
                var featureAccessTime = featureAccessTimes?.FirstOrDefault(x => x.LessonId == item);
                if (featureAccessTime != null)
                {
                    managerUnit.TimeSpent = featureAccessTime.AccessTime;
                    managerUnit.LastVisited = featureAccessTime.LastVisited;
                    managerUnit.Visit = featureAccessTime.Visit;
                }

                managerCourseProgress.Add(managerUnit);
            }

            methodResult.Result = managerCourseProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<LessonManagerProgressModel> GetManagerLesson(Guid? lessonId, Guid? studentId, EnumCourseType type, Guid userId)
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
                managerUnit.Percent = NumberHelper.ConvertDouble(list.Average());
                managerUnit.ContentCompleted = string.Format("{0} / {1}", list.Sum(), 3);
            }
            return managerUnit;
        }
    }
}
