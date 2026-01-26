// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.DashboardQuery.V1i1
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.DashboardModels;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetDashboardHomeQuery : IRequest<MethodResult<DashboardHomeModel>>
    {
    }

    public class GetDashboardHomeQueryHandler : IRequestHandler<GetDashboardHomeQuery, MethodResult<DashboardHomeModel>>
    {
        private readonly IUserService _userService;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseModuleRepository _courseModuleRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonNoteRepository _lessonNoteRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public GetDashboardHomeQueryHandler(IUserService userService
            , ICourseRepository courseRepository
            , ICourseModuleRepository courseModuleRepository
            , IUnitRepository unitRepository
            , ICourseResultRepository courseResultRepository
            , ILessonResultRepository lessonResultRepository
            , ILessonNoteRepository lessonNoteRepository
            , IUnitLessonRepository unitLessonRepository
            , IMapper mapper
            , AuthContext authContext)
        {
            _userService = userService;
            _courseRepository = courseRepository;
            _courseModuleRepository = courseModuleRepository;
            _unitRepository = unitRepository;
            _courseResultRepository = courseResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _lessonNoteRepository = lessonNoteRepository;
            _unitLessonRepository = unitLessonRepository;
            _mapper = mapper;
            _authContext = authContext;
        }

        public async Task<MethodResult<DashboardHomeModel>> Handle(GetDashboardHomeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<DashboardHomeModel>();
            DashboardHomeModel dashboardHome = new DashboardHomeModel();

            var methodStudent = await GetStudentModelAsync();
            if (!methodStudent.IsOK)
            {
                methodResult.AddErrorBadRequest(methodStudent.ErrorMessages);
                return methodResult;
            }
            var student = methodStudent.Result ?? new StudentModel();
            var methodCourse = await GetCourseResultAsync(student.Id);
            if (!methodCourse.IsOK)
            {
                methodResult.AddErrorBadRequest(methodCourse.ErrorMessages);
                return methodResult;
            }
            var courseResult = methodCourse.Result ?? new CourseResult();

            var (totalNote, lessonResult) = await GetTotalNoteAndLessonResultAsync(courseResult.CourseId, courseResult.StudentId, cancellationToken);

            dashboardHome.TotalCoin = student.NumberOfTokenReceived;
            dashboardHome.CountCompleteLesson = await GetTotalLessonCompleteAsync(courseResult, cancellationToken);
            dashboardHome.TotalLesson = await GetTotalLessonAsync(courseResult, cancellationToken);
            dashboardHome.TotalNote = totalNote;
            dashboardHome.LessonResult = _mapper.Map<LessonResultModel>(lessonResult);

            var dailyStreakResult = await _userService.GetDailyStreak(courseResult.StudentId);
            if (!dailyStreakResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(dailyStreakResult));
                return methodResult;
            }

            var dailyStreak = dailyStreakResult?.Content?.Result;
            if (dailyStreak != null)
            {
                dashboardHome.NumberOfDaysStreak = dailyStreak.NumberOfDaysStreak;
                dashboardHome.IsDaysStreakIncrease = dailyStreak.IsDaysStreakIncrease;
            }
            methodResult.Result = dashboardHome;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<(int, LessonResult?)> GetTotalNoteAndLessonResultAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken)
        {
            var lessonNoteQuerys = await (from lessonResult in _lessonResultRepository.ReadQueryable
                                          join lessonNote in _lessonNoteRepository.ReadQueryable on lessonResult.Id equals lessonNote.LessonResultId
                                          where lessonResult.CourseId == courseId && lessonResult.StudentId == studentId
                                          select new
                                          {
                                              LessonNote = lessonNote,
                                              LessonResult = lessonResult,
                                          }).AsNoTracking().ToListAsync(cancellationToken);
            return (lessonNoteQuerys.Count, lessonNoteQuerys.OrderByDescending(x => (x.LessonNote.UpdatedDate ?? x.LessonNote.CreatedDate)).Select(x => x.LessonResult).FirstOrDefault());
        }

        private async Task<int> GetTotalLessonCompleteAsync(CourseResult courseResult, CancellationToken cancellationToken)
        {
            return await _lessonResultRepository.Queryable.Where(x => x.StudentId == courseResult.StudentId && x.CourseResultId == courseResult.Id)
                                                .Where(x => x.Status == EnumResultStatus.Done)
                                                .CountAsync(cancellationToken);
        }

        private async Task<int> GetTotalLessonAsync(CourseResult courseResult, CancellationToken cancellationToken)
        {
            var originalUnitIds = await _courseModuleRepository.ReadQueryable.Where(x => x.CourseId == courseResult.CourseId && x.CourseConfigType == EnumCourseConfigType.Unit)
                .Select(x => x.OriginalId)
                .ToListAsync(cancellationToken);

            var units = await _unitRepository.ReadQueryable
                .Include(x => x.UnitModules)
                .Where(x => originalUnitIds.Contains(x.OriginalId) && x.VersionStatus == EnumVersionStatus.LastVersion)
                .ToListAsync(cancellationToken);

            var originalLessonIds = units.SelectMany(x => x.UnitModules)
                .Where(x => x.UnitConfigType == EnumUnitConfigType.Lesson)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            return originalLessonIds.Count;
        }

        private async Task<MethodResult<StudentModel>> GetStudentModelAsync()
        {
            var methodResult = new MethodResult<StudentModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
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
            methodResult.Result = student;
            return methodResult;
        }

        private async Task<MethodResult<CourseResult>> GetCourseResultAsync(Guid studentId)
        {
            var methodResult = new MethodResult<CourseResult>();
            var courseResult = await _courseResultRepository.Queryable.Include(x => x.Course)
                                                            .FirstOrDefaultAsync(x => x.StudentId == studentId && x.WorkingStatus == EnumWorkingStatus.Active);
            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
                return methodResult;
            }
            methodResult.Result = courseResult;
            return methodResult;
        }
    }
}
