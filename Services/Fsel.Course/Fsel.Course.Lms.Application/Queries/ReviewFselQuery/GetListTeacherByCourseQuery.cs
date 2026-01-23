// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
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
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoRepository _videoRepository;

        public GetListTeacherByCourseQueryHandler(IUserService userService,
            ICourseRepository courseRepository,
            ILessonResultRepository lessonResultRepository,
            IVideoResultRepository videoResultRepository,
            IVideoRepository videoRepository,
            ICourseResultRepository courseResultRepository)
        {
            _userService = userService;
            _courseRepository = courseRepository;
            _lessonResultRepository = lessonResultRepository;
            _videoResultRepository = videoResultRepository;
            _videoRepository = videoRepository;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<IList<TeacherModel>>> Handle(GetListTeacherByCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<TeacherModel>> methodResult = new MethodResult<IList<TeacherModel>>();
            var course = from baseQ in _videoRepository.Queryable
                         join vr in _videoResultRepository.Queryable on baseQ.Id equals vr.VideoId
                         join lr in _lessonResultRepository.Queryable on vr.LessonResultId equals lr.Id
                         join cr in _courseResultRepository.Queryable on lr.CourseResultId equals cr.Id
                         join c in _courseRepository.Queryable on cr.CourseId equals c.Id
                         where c.Id == request.CourseId
                         select new
                         {
                             TeacherId = baseQ.TeacherId
                         };

            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var teacherIds = await course.Select(x => x.TeacherId).Distinct().ToListAsync(cancellationToken);
            var teacherResults = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherIds });
            if (!teacherResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var teachers = teacherResults.Content?.Result;
            methodResult.Result = teachers?.OrderBy(x => x.CreatedDate).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
