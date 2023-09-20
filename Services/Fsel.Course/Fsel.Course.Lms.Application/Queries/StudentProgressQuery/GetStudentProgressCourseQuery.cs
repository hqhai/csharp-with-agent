// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetStudentProgressCourseQuery : IRequest<MethodResult<CourseProgressModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetManageStudentCourseQueryHandler : IRequestHandler<GetStudentProgressCourseQuery, MethodResult<CourseProgressModel>>
    {
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly IOrderService _orderService;
        private readonly ICourseRepository _courseRepository;
        private readonly ITrainingService _trainingService;

        public GetManageStudentCourseQueryHandler(IUserService userService, ISystemService systemService, IOrderService orderService, ICourseRepository courseRepository, ITrainingService trainingService)
        {
            _userService = userService;
            _systemService = systemService;
            _orderService = orderService;
            _courseRepository = courseRepository;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<CourseProgressModel>> Handle(GetStudentProgressCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseProgressModel> methodResult = new MethodResult<CourseProgressModel>();
            CourseProgressModel courseProgress = new CourseProgressModel();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { request.StudentId });
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }

            var student = studentResults?.Content?.Result?.FirstOrDefault();
            var packageResults = await _orderService.GetPackages();
            if (!packageResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallOrderServiceError), nameof(packageResults));
                return methodResult;
            }
            var packages = packageResults.Content?.Result;
            var @classResults = await _trainingService.GetListClassByStudentIdAsync(student?.Id ?? default);
            if (!@classResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallTrainingServiceError), nameof(@classResults));
                return methodResult;
            }
            var @classes = @classResults.Content?.Result;

            courseProgress.FullName = student?.Human?.FullName;
            courseProgress.StudentId = request.StudentId;
            var featureAccessTimes = new List<FeatureAccessTimeModel>();
            var listCourseStudentProgress = new List<CourseStudentProgressModel>();
            if (@classes != null && @classes.Any())
            {
                var courseIds = @classes.OrderBy(x => x.CreatedDate).Select(x => x.CourseId).ToList();
                var courses = await _courseRepository.GetByIdsAsync(courseIds);
                if (courses == null || !courses.Any())
                {
                    methodResult.Result = default;
                    methodResult.StatusCode = StatusCodes.Status200OK;
                    return methodResult;
                }
                var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesByCourseIdsAsync(new FeatureAccessTimesByCourseIdsQueryModel { CourseIds = courseIds, UserId = student?.Human?.UserId ?? default });
                if (!featureAccessTimeResults.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResults));
                    return methodResult;
                }
                featureAccessTimes = featureAccessTimeResults.Content?.Result?.ToList();
                foreach (var item in courses)
                {
                    CourseStudentProgressModel courseStudentProgress = new CourseStudentProgressModel();
                    var (currentProgress, progress, displayOrderUnit, displayOrderLesson) = await _courseRepository.GetContentCompleted(item.Id, item.CourseType, request.StudentId);
                    courseStudentProgress.ContentCompleted = string.Format("{0} / {1}", currentProgress, progress);
                    courseStudentProgress.CourseName = item.Code;
                    courseStudentProgress.CourseId = item.Id;
                    var featureAccessTime = featureAccessTimes?.FirstOrDefault(x => x.CourseId == item.Id);
                    courseStudentProgress.TimeSpent = featureAccessTime?.AccessTime ?? default;
                    var @class = @classes.FirstOrDefault(x => x.CourseId == item.Id);
                    if (@class != null)
                    {
                        courseStudentProgress.ClassId = @class.Id;
                        courseStudentProgress.CodeClass = @class.Code;
                        var package = packages?.FirstOrDefault(x => x.Id == @class.PackageId);
                        courseStudentProgress.StartDate = @class.TimeStart;
                        courseStudentProgress.EndDate = @class.TimeEnd;
                        courseStudentProgress.PackageId = @class.PackageId;
                        courseStudentProgress.PackageCode = package?.Code ?? default;
                    }
                    listCourseStudentProgress.Add(courseStudentProgress);
                }
            }
            courseProgress.CourseStudentProgressModels = listCourseStudentProgress;
            methodResult.Result = courseProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
