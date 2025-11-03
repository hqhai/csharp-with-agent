// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentLearningReportQuery : IRequest<MethodResult<StudentCourseProgressModel>>
    {
        public Guid UserId { get; set; }
    }

    public class GetStudentLearningReportQueryHandler : IRequestHandler<GetStudentLearningReportQuery, MethodResult<StudentCourseProgressModel>>
    {
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private const string CourseDone = "Đã hoàn thành khóa học";

        public GetStudentLearningReportQueryHandler(ManagerProgressHelper managerProgressHelper,
            ISystemService systemService,
            IUserService userService,
            ICourseResultRepository courseResultRepository)
        {
            _managerProgressHelper = managerProgressHelper;
            _systemService = systemService;
            _userService = userService;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<StudentCourseProgressModel>> Handle(GetStudentLearningReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentCourseProgressModel> methodResult = new MethodResult<StudentCourseProgressModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId);
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                return methodResult;
            }

            var courseResult = await _courseResultRepository.Queryable.Where(x => x.StudentId == student.Id)
                                                            .Where(x => x.WorkingStatus == EnumWorkingStatus.Active)
                                                            .FirstOrDefaultAsync(cancellationToken);

            var courseId = courseResult?.CourseId ?? student.CourseId;
            if (!courseId.HasValue)
            {
                return methodResult;
            }
            var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeByUserIdsAsync(new List<Guid> { request.UserId });
            var featureAccessTime = featureAccessTimeResult?.Content?.Result?.FirstOrDefault();

            var courseCompletes = await _managerProgressHelper.GetProgressCompleteLessonAsync(new List<CourseResultModel>
            {
                new CourseResultModel { CourseId = courseId.Value, StudentId = student.Id  }
            });

            var courseComplete = courseCompletes.FirstOrDefault();
            var lastDate = featureAccessTime?.LastVisited;
            methodResult.Result = new StudentCourseProgressModel
            {
                StudentId = student.Id,
                CourseId = courseId.Value,
                CurrentLessonIndex = courseComplete?.TotalLessonDone ?? default,
                TotalLessons = courseComplete?.TotalLesson ?? default,
                UnitName = courseResult?.Status == EnumResultStatus.Done ? CourseDone : courseComplete?.UnitName,
                IsCourseCompleted = courseResult?.Status == EnumResultStatus.Done,
                StartDate = courseResult != null && courseResult.ProcessDate.HasValue ? courseResult.ProcessDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam) : null,
                TotalActiveDuration = featureAccessTime?.AccessTime,
                LastAccessedDate = lastDate.HasValue ? lastDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam) : default,
            };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
