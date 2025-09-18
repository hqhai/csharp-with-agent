// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.DashboardQuery.V1i1
{
    using System.Linq.Dynamic.Core;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.DashboardModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetOverallHomeQuery : IRequest<MethodResult<OverallHomeModel>>
    {
    }

    public class GetOverallHomeQueryHandler : IRequestHandler<GetOverallHomeQuery, MethodResult<OverallHomeModel>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;

        public GetOverallHomeQueryHandler(
            IUserService userService,
            ICourseResultRepository courseResultRepository,
            IMapper mapper,
            AuthContext authContext,
            ManagerProgressHelper managerProgressHelper,
            IUnitResultRepository unitResultRepository,
            ILessonResultRepository lessonResultRepository,
            IMockTestResultRepository mockTestResultRepository)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _mapper = mapper;
            _authContext = authContext;
            _managerProgressHelper = managerProgressHelper;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
        }

        public async Task<MethodResult<OverallHomeModel>> Handle(GetOverallHomeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<OverallHomeModel>();
            var overallHome = new OverallHomeModel();

            // 1) Student
            var methodStudent = await GetStudentModelAsync();
            if (!methodStudent.IsOK)
            {
                methodResult.AddErrorBadRequest(methodStudent.ErrorMessages);
                return methodResult;
            }
            var student = methodStudent.Result!;

            // 2) CourseResult (đang Active)
            var methodCourse = await GetCourseResultAsync(student.Id, cancellationToken);
            if (!methodCourse.IsOK)
            {
                methodResult.AddErrorBadRequest(methodCourse.ErrorMessages);
                return methodResult;
            }
            var courseResult = methodCourse.Result!;

            // 3) Tiến độ tổng thể khoá học
            var (currentProgress, progress) =
                await _managerProgressHelper.GetCompleteCourseAsync(_mapper.Map<CourseResultModel>(courseResult));
            overallHome.ProgressPercent = NumberHelper.GetPercent(currentProgress, progress);

            // 4) UnitResult mới nhất
            var unitResult = await _unitResultRepository.Queryable.Include(x => x.Unit)
                .Where(x => x.StudentId == student.Id && x.CourseId == courseResult.CourseId)
                .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            // Nếu chưa có UnitResult => không có dữ liệu để tính % của unit hiện tại
            if (unitResult == null)
            {
                methodResult.Result = overallHome;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            overallHome.Name = unitResult.Unit?.Name;

            // 5) Lấy song song các status của LessonResult & MockTestResult trong Unit hiện tại
            var lessonStatuses = await _lessonResultRepository.Queryable
                .Where(x => x.UnitId == unitResult.UnitId
                            && x.CourseId == courseResult.CourseId
                            && x.StudentId == student.Id)
                .AsNoTracking()
                .Select(x => x.Status)
                .ToListAsync(cancellationToken);

            var mockStatuses = await _mockTestResultRepository.Queryable
                .Where(x => x.UnitId == unitResult.UnitId
                            && x.CourseId == courseResult.CourseId
                            && x.StudentId == student.Id)
                .AsNoTracking()
                .Select(x => x.Status)
                .ToListAsync(cancellationToken);

            // 6) Gộp & tính %
            var allStatuses = lessonStatuses.Concat(mockStatuses).ToList();
            var total = allStatuses.Count;
            var done = allStatuses.Count(s => s == EnumResultStatus.Done);
            overallHome.ProgressUnitPercent = NumberHelper.GetPercent(done, total);
            // 7) Trả kết quả
            methodResult.Result = overallHome;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MethodResult<StudentModel>> GetStudentModelAsync()
        {
            var methodResult = new MethodResult<StudentModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId /*, ct nếu method hỗ trợ */);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            methodResult.Result = student;
            return methodResult;
        }

        private async Task<MethodResult<CourseResult>> GetCourseResultAsync(Guid studentId, CancellationToken ct)
        {
            var methodResult = new MethodResult<CourseResult>();

            var courseResult = await _courseResultRepository.Queryable
                .Include(x => x.Course)
                .Where(x => x.StudentId == studentId && x.WorkingStatus == EnumWorkingStatus.Active)
                .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);

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
