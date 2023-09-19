// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManageProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetManageStudentProgressCourseQuery : IRequest<MethodResult<ManagerCourseProgressModel>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class GetManageStudentProgressCourseQueryHandler : IRequestHandler<GetManageStudentProgressCourseQuery, MethodResult<ManagerCourseProgressModel>>
    {
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly IOrderService _orderService;
        private readonly ICourseRepository _courseRepository;
        private readonly ITrainingService _trainingService;

        public GetManageStudentProgressCourseQueryHandler(IUserService userService, ISystemService systemService, IOrderService orderService, ICourseRepository courseRepository, ITrainingService trainingService)
        {
            _userService = userService;
            _systemService = systemService;
            _orderService = orderService;
            _courseRepository = courseRepository;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<ManagerCourseProgressModel>> Handle(GetManageStudentProgressCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ManagerCourseProgressModel> methodResult = new MethodResult<ManagerCourseProgressModel>();
            ManagerCourseProgressModel managerCourseProgress = new ManagerCourseProgressModel();
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
            var courseIds = new List<Guid> { course.Id };
            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesByCourseIdsAsync(courseIds, student?.Human?.UserId ?? default);
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
            managerCourseProgress.TotalVisit = featureAccessTime?.TotalVisit ?? default;
            managerCourseProgress.TimeSpent = featureAccessTime?.AccessTime ?? default;
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
            methodResult.Result = default;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
