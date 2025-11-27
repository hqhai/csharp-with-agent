// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetStudentProgressCourseQuery : IRequest<MethodResult<CourseStudentProgressModel>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class GetStudentProgressCourseQueryHandler : IRequestHandler<GetStudentProgressCourseQuery, MethodResult<CourseStudentProgressModel>>
    {
        private readonly IUserService _userService;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ISystemService _systemService;
        private readonly IOrderService _orderService;
        private readonly ICourseRepository _courseRepository;
        private readonly ITrainingService _trainingService;

        public GetStudentProgressCourseQueryHandler(IUserService userService, ManagerProgressHelper managerProgressHelper, ICourseResultRepository courseResultRepository, ISystemService systemService, IOrderService orderService, ICourseRepository courseRepository, ITrainingService trainingService)
        {
            _userService = userService;
            _managerProgressHelper = managerProgressHelper;
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
            var studentResult = await _userService.GetUserByStudentId(request.StudentId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student.Id;
            var userId = student.UserId;
            //var packageResults = await _orderService.GetPackages();
            //if (!packageResults.IsSuccessStatusCode)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallOrderServiceError), nameof(packageResults));
            //    return methodResult;
            //}
            //var packages = packageResults.Content?.Result;
            var @classResults = await _trainingService.GetListClassByStudentIdAsync(student.Id);
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

            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesAsync(new FeatureAccessTimesQueryModel
            {
                FeatureAccessTimes = new List<FeatureAccessTimeQueryModel>
                {
                    new FeatureAccessTimeQueryModel
                    {
                        CourseId = request.CourseId,
                        UserId = userId
                    },
                },
                UserId = userId
            });
            if (!featureAccessTimeResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResults));
                return methodResult;
            }
            var featureAccessTimes = featureAccessTimeResults.Content?.Result;
            var featureAccessTime = featureAccessTimes?.FirstOrDefault();

            var courseProgress = await _managerProgressHelper.GetCourseManagerAsync(course.Id, studentId, cancellationToken);
            if (courseProgress == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseProgress));
                return methodResult;
            }

            var @class = @classes?.FirstOrDefault(x => x.CourseId == course.Id);
            if (@class != null)
            {
                courseProgress.ClassId = @class.Id;
                courseProgress.CodeClass = @class.Code;
            }
            if (featureAccessTime != null)
            {
                courseProgress.Visit = featureAccessTime.Visit;
                courseProgress.TimeSpent = featureAccessTime.AccessTime;
            }
            //var package = packages?.FirstOrDefault(x => x.Id == student.PackageId);
            //if (package != null)
            //{
            //    courseProgress.PackageId = package.Id;
            //    courseProgress.PackageCode = package.Code;
            //}
            methodResult.Result = courseProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
