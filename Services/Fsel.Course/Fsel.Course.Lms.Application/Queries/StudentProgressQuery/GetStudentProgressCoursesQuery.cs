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

    public class GetStudentProgressCoursesQuery : IRequest<MethodResult<IList<CourseStudentProgressModel>>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetStudentProgressCoursesQueryHandler : IRequestHandler<GetStudentProgressCoursesQuery, MethodResult<IList<CourseStudentProgressModel>>>
    {
        private readonly IUserService _userService;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ISystemService _systemService;
        private readonly IOrderService _orderService;
        private readonly ICourseRepository _courseRepository;
        private readonly ITrainingService _trainingService;

        public GetStudentProgressCoursesQueryHandler(IUserService userService, ManagerProgressHelper managerProgressHelper, ICourseResultRepository courseResultRepository, ISystemService systemService, IOrderService orderService, ICourseRepository courseRepository, ITrainingService trainingService)
        {
            _userService = userService;
            _managerProgressHelper = managerProgressHelper;
            _courseResultRepository = courseResultRepository;
            _systemService = systemService;
            _orderService = orderService;
            _courseRepository = courseRepository;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<IList<CourseStudentProgressModel>>> Handle(GetStudentProgressCoursesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CourseStudentProgressModel>> methodResult = new MethodResult<IList<CourseStudentProgressModel>>();
            IList<CourseStudentProgressModel> courseProgress = new List<CourseStudentProgressModel>();
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

            var userId = student.UserId;
            //var packageResults = await _orderService.GetPackages();
            //if (!packageResults.IsSuccessStatusCode)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallOrderServiceError), nameof(packageResults));
            //    return methodResult;
            //}
            //var packages = packageResults.Content?.Result;
            var @classResults = await _trainingService.GetListClassByStudentIdAsync(request.StudentId);
            if (!@classResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallTrainingServiceError), nameof(@classResults));
                return methodResult;
            }
            var @classes = @classResults.Content?.Result;
            if (@classes == null || !@classes.Any())
            {
                return methodResult;
            }

            var courseIds = @classes.OrderBy(x => x.CreatedDate).Select(x => x.CourseId).ToList();
            var courses = await _courseRepository.GetByIdsAsync(courseIds);
            if (courses == null || !courses.Any())
            {
                return methodResult;
            }
            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesAsync(new FeatureAccessTimesQueryModel
            {
                FeatureAccessTimes = courseIds.Select(x => new FeatureAccessTimeQueryModel
                {
                    CourseId = x,
                    UserId = userId
                }).ToList(),
                UserId = userId
            });
            if (!featureAccessTimeResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResults));
                return methodResult;
            }
            var featureAccessTimes = featureAccessTimeResults.Content?.Result?.ToList();
            foreach (var item in courses)
            {
                var courseStudentProgress = await _managerProgressHelper.GetCourseManagerAsync(item.Id, student.Id, cancellationToken);
                if (courseStudentProgress == null)
                {
                    continue;
                }
                if (featureAccessTimes != null)
                {
                    var featureAccessTime = featureAccessTimes.FirstOrDefault(x => x.CourseId == item.Id);
                    courseStudentProgress.TimeSpent = featureAccessTime?.AccessTime ?? default;
                    courseStudentProgress.Visit = featureAccessTime?.Visit ?? default;
                }
                //var package = packages?.FirstOrDefault(x => x.Id == student.PackageId);
                //if (package != null)
                //{
                //    courseStudentProgress.PackageId = package.Id;
                //    courseStudentProgress.PackageCode = package.Code;
                //}
                var @class = @classes.FirstOrDefault(x => x.CourseId == item.Id);
                if (@class != null)
                {
                    courseStudentProgress.ClassId = @class.Id;
                    courseStudentProgress.CodeClass = @class.Code;
                }
                courseProgress.Add(courseStudentProgress);
            }
            methodResult.Result = courseProgress.OrderByDescending(x => x.UpdatedDate).ThenByDescending(x => x.CreatedDate).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
