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
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProgressCourseQuery : IRequest<MethodResult<CourseStudentProgressModel>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class GetStudentManageProgressCourseQueryHandler : IRequestHandler<GetStudentProgressCourseQuery, MethodResult<CourseStudentProgressModel>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ISystemService _systemService;
        private readonly IOrderService _orderService;
        private readonly ICourseRepository _courseRepository;
        private readonly ITrainingService _trainingService;

        public GetStudentManageProgressCourseQueryHandler(IUserService userService, ICourseResultRepository courseResultRepository, ISystemService systemService, IOrderService orderService, ICourseRepository courseRepository, ITrainingService trainingService)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _systemService = systemService;
            _orderService = orderService;
            _courseRepository = courseRepository;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<CourseStudentProgressModel>> Handle(GetStudentProgressCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseStudentProgressModel> methodResult = new MethodResult<CourseStudentProgressModel>();
            CourseStudentProgressModel managerCourseProgress = new CourseStudentProgressModel();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { request.StudentId });
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }

            var student = studentResults?.Content?.Result?.FirstOrDefault();
            var studentId = student?.Id;
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
            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesByCourseIdsAsync(new FeatureAccessTimesQueryModel { CourseIds = new List<Guid> { request.CourseId }, UserId = student?.Human?.UserId ?? default });
            if (!featureAccessTimeResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResults));
                return methodResult;
            }
            var featureAccessTimes = featureAccessTimeResults.Content?.Result;
            var featureAccessTime = featureAccessTimes?.FirstOrDefault();
            var @class = @classes?.FirstOrDefault(x => x.CourseId == course.Id);
            var courseResult = await _courseResultRepository.Queryable.Include(x => x.Course).FirstOrDefaultAsync(x => x.StudentId == studentId && x.CourseId == course.Id, cancellationToken);
            if (courseResult != null)
            {
                var courseResultModel = new CourseResultModel
                {
                    CourseType = courseResult.Course?.CourseType,
                    CourseId = courseResult.CourseId,
                    StudentId = courseResult.StudentId
                };
                var (currentProgress, progress) = await _courseRepository.GetContentComplete(courseResultModel);
                managerCourseProgress.ContentCompleted = string.Format("{0} / {1}", currentProgress, progress);
            }
            managerCourseProgress.CourseName = course.Code;
            managerCourseProgress.CourseId = course.Id;
            if (featureAccessTime != null)
            {
                managerCourseProgress.Visit = featureAccessTime.Visit;
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
