// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery.V1i2
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.CourseResultCmd.V1i2;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Course = Domain.Entities.Course;

    public class GetCourseQuery : IRequest<MethodResult<CourseModel>>
    {
    }

    public class GetCourseQueryHandler : IRequestHandler<GetCourseQuery, MethodResult<CourseModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ICourseCachingService _courseCachingService;

        public GetCourseQueryHandler(AuthContext authContext,
            IMediator mediator,
            IUserService userService,
            ICourseResultRepository courseResultRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ICourseCachingService courseCachingService)
        {
            _authContext = authContext;
            _mediator = mediator;
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _courseCachingService = courseCachingService;
        }

        public async Task<MethodResult<CourseModel>> Handle(GetCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CourseModel>();
            var student = await GetStudentModelAsync(methodResult);
            if (student == null || !methodResult.IsOK)
            {
                return methodResult;
            }
            var courseResult = await GetAndSaveAsync(student, cancellationToken);
            var course = await GetCourseAsync(student.CourseId);
            if (course != null)
            {
                var courseModel = _mapper.Map<CourseModel>(course);
                courseModel.CourseResult = courseResult;
                methodResult.Result = courseModel;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<StudentModel?> GetStudentModelAsync(MethodResult<CourseModel> methodResult)
        {
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return null;
            }

            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return null;
            }

            if (!student.CourseId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.CourseId));
                return null;
            }

            return student;
        }

        private async Task<CourseResultModel?> GetAndSaveAsync(StudentModel student, CancellationToken cancellationToken)
        {
            var courseResult = await _courseResultRepository.ReadQueryable
                                                            .Where(x => x.CourseId == student.CourseId)
                                                            .Where(x => x.StudentId == student.Id && x.WorkingStatus == EnumWorkingStatus.Active)
                                                            .FirstOrDefaultAsync(cancellationToken);
            if (courseResult == null && student.CourseId.HasValue)
            {
                var result = await _mediator.Send(new SaveCourseResultCommand { CourseId = student.CourseId.Value, StudentId = student.Id }, cancellationToken);
                return result.Result;
            }
            return _mapper.Map<CourseResultModel>(courseResult);
        }

        private async Task<Course?> GetCourseAsync(Guid? id)
        {
            if (!id.HasValue)
            {
                return null;
            }

            return await _courseCachingService.GetOrSetAsync(id.Value.ToString(), async (ctx, _) =>
            {
                var course = await _courseRepository.ReadQueryable
                                                    .Include(x => x.CourseTeachers)
                                                    .Include(x => x.Level)
                                                    .Where(x => x.Id == id)
                                                    .FirstOrDefaultAsync(_);
                return course;
            });
        }
    }
}
