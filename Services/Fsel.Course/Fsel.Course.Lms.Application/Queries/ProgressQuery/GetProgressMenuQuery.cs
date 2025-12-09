// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetProgressMenuQuery : IRequest<MethodResult<ProgressMenuModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetProgressMenuQueryHandler : IRequestHandler<GetProgressMenuQuery, MethodResult<ProgressMenuModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUserService _userService;

        public GetProgressMenuQueryHandler(AuthContext authContext
            , IClassForumResultRepository classForumResultRepository
            , IUnitResultRepository unitResultRepository
            , ICourseResultRepository courseResultRepository
            , ICourseRepository courseRepository
            , ILessonResultRepository lessonResultRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _classForumResultRepository = classForumResultRepository;
            _unitResultRepository = unitResultRepository;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _lessonResultRepository = lessonResultRepository;
            _userService = userService;
        }

        public async Task<MethodResult<ProgressMenuModel>> Handle(GetProgressMenuQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ProgressMenuModel> methodResult = new MethodResult<ProgressMenuModel>();
            ProgressMenuModel progressMenu = new ProgressMenuModel();

            var method = await ValidateAsync(request);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var (course, courseResult) = method.Result;
            if (courseResult == null)
            {
                methodResult.Result = progressMenu;
                return methodResult;
            }
            var lessonResultIds = await _lessonResultRepository.Queryable.Where(x => x.StudentId == courseResult.StudentId && x.CourseId == course.Id).Select(x => x.Id).ToListAsync(cancellationToken);
            progressMenu.NumberOfUnitDone = await _unitResultRepository.Queryable.Where(x => x.CourseId == course.Id && x.StudentId == courseResult.StudentId && x.Status == EnumResultStatus.Done).CountAsync(cancellationToken);
            progressMenu.NumberOfPostsCreated = await _classForumResultRepository.Queryable.Where(x => lessonResultIds.Contains(x.LessonResultId))
                                                                                           .Where(x => x.Status.HasValue)
                                                                                           .CountAsync(cancellationToken);
            var dailyStreakResult = await _userService.GetDailyStreak(courseResult.StudentId);
            if (!dailyStreakResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(dailyStreakResult));
                return methodResult;
            }

            var dailyStreak = dailyStreakResult?.Content?.Result;
            if (dailyStreak != null)
            {
                progressMenu.NumberOfDaysStreak = dailyStreak.NumberOfDaysStreak;
                progressMenu.IsDaysStreakIncrease = dailyStreak.IsDaysStreakIncrease;
            }

            methodResult.Result = progressMenu;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MethodResult<(Course, CourseResult?)>> ValidateAsync(GetProgressMenuQuery request)
        {
            var methodResult = new MethodResult<(Course, CourseResult?)>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == student.Id && x.CourseId == course.Id);

            methodResult.Result = (course, courseResult);
            return methodResult;
        }
    }
}
