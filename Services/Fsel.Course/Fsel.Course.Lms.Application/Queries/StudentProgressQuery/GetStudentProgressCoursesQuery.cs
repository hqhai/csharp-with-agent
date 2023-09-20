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
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProgressCoursesQuery : IRequest<MethodResult<IList<CourseStudentProgressModel>>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetManageStudentCourseQueryHandler : IRequestHandler<GetStudentProgressCoursesQuery, MethodResult<IList<CourseStudentProgressModel>>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ISystemService _systemService;
        private readonly IOrderService _orderService;
        private readonly ICourseRepository _courseRepository;
        private readonly ITrainingService _trainingService;

        public GetManageStudentCourseQueryHandler(IUserService userService, ICourseResultRepository courseResultRepository, ISystemService systemService, IOrderService orderService, ICourseRepository courseRepository, ITrainingService trainingService)
        {
            _userService = userService;
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

                var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesByCourseIdsAsync(new FeatureAccessTimesQueryModel { CourseIds = courseIds, UserId = student?.Human?.UserId ?? default });
                if (!featureAccessTimeResults.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResults));
                    return methodResult;
                }
                var featureAccessTimes = featureAccessTimeResults.Content?.Result?.ToList();
                foreach (var item in courses)
                {
                    var courseResult = await _courseResultRepository.Queryable.Include(x => x.Course).FirstOrDefaultAsync(x => x.StudentId == studentId && x.CourseId == item.Id, cancellationToken);
                    CourseStudentProgressModel courseStudentProgress = new CourseStudentProgressModel();
                    if (courseResult != null)
                    {
                        var courseResultModel = new CourseResultModel
                        {
                            CourseType = courseResult.Course?.CourseType,
                            CourseId = courseResult.CourseId,
                            StudentId = courseResult.StudentId
                        };
                        var (currentProgress, progress) = await _courseRepository.GetContentComplete(courseResultModel);
                        courseStudentProgress.ContentCompleted = string.Format("{0} / {1}", currentProgress, progress);
                    }
                    courseStudentProgress.CourseName = item.Code;
                    courseStudentProgress.CourseId = item.Id;
                    if (featureAccessTimes != null)
                    {
                        var featureAccessTime = featureAccessTimes.FirstOrDefault(x => x.CourseId == item.Id);
                        courseStudentProgress.TimeSpent = featureAccessTime?.AccessTime ?? default;
                        courseStudentProgress.Visit = featureAccessTime?.Visit ?? default;
                    }
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
                    courseProgress.Add(courseStudentProgress);
                }
            }
            methodResult.Result = courseProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
