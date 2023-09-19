// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManageProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetManageStudentProgressQuery : IRequest<MethodResult<CourseProgressModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetListLessonQueryHandler : IRequestHandler<GetManageStudentProgressQuery, MethodResult<CourseProgressModel>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ITrainingService _trainingService;

        public GetListLessonQueryHandler(IUserService userService, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, ITrainingService trainingService)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<CourseProgressModel>> Handle(GetManageStudentProgressQuery request, CancellationToken cancellationToken)
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
            courseProgress.FullName = student?.Human?.FullName;
            courseProgress.StudentId = request.StudentId;
            var courseResults = await _courseResultRepository.Queryable.Where(x => x.StudentId == request.StudentId).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);
            if (courseResults == null || courseResults.Any())
            {
                methodResult.Result = default;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var courses = await _courseRepository.GetByIdsAsync(courseResults.Select(x => x.CourseId).ToList());
            if (courses == null || courses.Any())
            {
                methodResult.Result = default;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            foreach (var item in courses)
            {
                //CourseStudentProgressModel courseStudentProgress = new CourseStudentProgressModel();
                //var (currentProgress, progress, displayOrderUnit, displayOrderLesson) = await _courseRepository.GetContentCompleted(item.Id, item.CourseType, request.StudentId);
                //courseStudentProgress.ContentCompleted = string.Format("{0} / {1}", currentProgress, progress);
            }
            methodResult.Result = default;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
