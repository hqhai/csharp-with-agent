// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListTeacherByCourseQuery : IRequest<MethodResult<IList<TeacherModel>>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetListTeacherByCourseQueryHandler : IRequestHandler<GetListTeacherByCourseQuery, MethodResult<IList<TeacherModel>>>
    {
        private readonly IUserService _userService;
        private readonly ICourseRepository _courseRepository;

        public GetListTeacherByCourseQueryHandler(IUserService userService, ICourseRepository courseRepository)
        {
            _userService = userService;
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<IList<TeacherModel>>> Handle(GetListTeacherByCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<TeacherModel>> methodResult = new MethodResult<IList<TeacherModel>>();
            var course = await _courseRepository.Queryable.Include(x => x.LessonResults)
                                                            .ThenInclude(x => x.VideoResult)
                                                            .ThenInclude(x => x!.Video)
                                                            .FirstOrDefaultAsync(x => x.Id == request.CourseId, cancellationToken);

            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotExist));
                return methodResult;
            }

            var teacherIds = course.LessonResults.Select(x => x.VideoResult).Select(x => x!.Video).Select(x => x!.TeacherId).ToList();
            var teacherResults = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherIds });
            if (!teacherResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var teachers = teacherResults.Content?.Result;
            methodResult.Result = teachers;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
