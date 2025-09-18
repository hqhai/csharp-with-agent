// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.DashboardQuery.V1i1
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.DashboardModels;
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
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly ILessonNoteRepository _lessonNoteRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public GetDashboardHomeQueryHandler(IUserService userService
            , ICourseResultRepository courseResultRepository
            , ILessonResultRepository lessonResultRepository
            , ICourseUnitMockTestRepository courseUnitMockTestRepository
            , ILessonNoteRepository lessonNoteRepository
            , IUnitLessonRepository unitLessonRepository
            , IMapper mapper
            , AuthContext authContext)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
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

            var (totalNote, lessonResult) = await GetTotalNoteAndLessonResultAsync(courseResult, cancellationToken);

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

        private async Task<(int, LessonResult?)> GetTotalNoteAndLessonResultAsync(CourseResult courseResult, CancellationToken cancellationToken)
        {
            var lessonNoteQuerys = await (from baseQ in _lessonResultRepository.Queryable
                                          join ln in _lessonNoteRepository.Queryable on baseQ.Id equals ln.LessonResultId
                                          where baseQ.CourseId == courseResult.CourseId && baseQ.StudentId == courseResult.StudentId
                                          select new
                                          {
                                              LessonNote = ln,
                                              LessonResult = baseQ,
                                          }).AsNoTracking().ToListAsync(cancellationToken);
            return (lessonNoteQuerys.Count, lessonNoteQuerys.OrderBy(x => (x.LessonNote.UpdatedDate ?? x.LessonNote.CreatedDate)).Select(x => x.LessonResult).FirstOrDefault());
        }

        private async Task<int> GetTotalLessonCompleteAsync(CourseResult courseResult, CancellationToken cancellationToken)
        {
            return await _lessonResultRepository.Queryable.Where(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId)
                                                .Where(x => x.Status == EnumResultStatus.Done)
                                                .CountAsync(cancellationToken);
        }

        private async Task<int> GetTotalLessonAsync(CourseResult courseResult, CancellationToken cancellationToken)
        {
            return await (from baseQ in _courseUnitMockTestRepository.Queryable
                          join ul in _unitLessonRepository.Queryable on baseQ.UnitId equals ul.UnitId
                          where baseQ.CourseId == courseResult.CourseId
                          select ul.LessonId).CountAsync(cancellationToken);
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
