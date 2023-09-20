// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
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

    public class GetStudentManageProgressCourseQuery : IRequest<MethodResult<CourseManagerProgressModel>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class GetStudentManageProgressCourseQueryHandler : IRequestHandler<GetStudentManageProgressCourseQuery, MethodResult<CourseManagerProgressModel>>
    {
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly IOrderService _orderService;
        private readonly ICourseRepository _courseRepository;
        private readonly ITrainingService _trainingService;

        public GetStudentManageProgressCourseQueryHandler(IUserService userService, ISystemService systemService, IOrderService orderService, ICourseRepository courseRepository, ITrainingService trainingService)
        {
            _userService = userService;
            _systemService = systemService;
            _orderService = orderService;
            _courseRepository = courseRepository;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<CourseManagerProgressModel>> Handle(GetStudentManageProgressCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseManagerProgressModel> methodResult = new MethodResult<CourseManagerProgressModel>();
            CourseManagerProgressModel managerCourseProgress = new CourseManagerProgressModel();
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

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesByCourseIdsAsync(new FeatureAccessTimesByCourseIdsQueryModel { CourseIds = new List<Guid> { request.CourseId }, UserId = student?.Human?.UserId ?? default });
            if (!featureAccessTimeResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResults));
                return methodResult;
            }
            var featureAccessTimes = featureAccessTimeResults.Content?.Result;
            var featureAccessTime = featureAccessTimes?.FirstOrDefault();
            var @class = @classes?.FirstOrDefault(x => x.CourseId == course.Id);
            var (currentProgress, progress, displayOrderUnit, displayOrderLesson) = await _courseRepository.GetContentCompleted(course.Id, course.CourseType, request.StudentId);
            managerCourseProgress.FullName = student?.Human?.FullName;
            managerCourseProgress.StudentId = request.StudentId;
            managerCourseProgress.ContentCompleted = string.Format("{0} / {1}", currentProgress, progress);
            managerCourseProgress.CourseName = course.Code;
            managerCourseProgress.CourseId = course.Id;
            if (featureAccessTime != null)
            {
                managerCourseProgress.TotalVisit = featureAccessTime.Visit;
                managerCourseProgress.TimeSpent = featureAccessTime.AccessTime;
            }
            if (@class != null)
            {
                var package = packages?.FirstOrDefault(x => x.Id == @class.PackageId);
                managerCourseProgress.ClassId = @class.Id;
                managerCourseProgress.CodeClass = @class.Code;
                managerCourseProgress.StartDate = @class.TimeStart;
                managerCourseProgress.EndDate = @class.TimeEnd;
                managerCourseProgress.PackageId = @class.PackageId;
                managerCourseProgress.PackageCode = package?.Code ?? default;
            }
            methodResult.Result = managerCourseProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
