// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.DashboardQuery.V1i1
{
    using System.Linq.Dynamic.Core;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.DashboardModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class GetOverallHomeQuery : MediatR.IRequest<MethodResult<OverallHomeModel>>
    {
    }

    public class GetOverallHomeQueryHandler : IRequestHandler<GetOverallHomeQuery, MethodResult<OverallHomeModel>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IDocumentResultRepository _documentResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;

        public GetOverallHomeQueryHandler(
            IUserService userService,
            ICourseResultRepository courseResultRepository,
            ICourseRepository courseRepository,
            IUnitRepository unitRepository,
            ILessonRepository lessonRepository,
            ITestGroupResultRepository testGroupResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IDocumentResultRepository documentResultRepository,
            IClassForumResultRepository classForumResultRepository,
            IVideoResultRepository videoResultRepository,
            IMapper mapper,
            AuthContext authContext,
            IUnitResultRepository unitResultRepository,
            ILessonResultRepository lessonResultRepository)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _lessonRepository = lessonRepository;
            _testGroupResultRepository = testGroupResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _documentResultRepository = documentResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _videoResultRepository = videoResultRepository;
            _mapper = mapper;
            _authContext = authContext;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<OverallHomeModel>> Handle(GetOverallHomeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<OverallHomeModel>();
            var overallHome = new OverallHomeModel();

            var methodStudent = await GetStudentModelAsync();
            if (!methodStudent.IsOK)
            {
                methodResult.AddErrorBadRequest(methodStudent.ErrorMessages);
                return methodResult;
            }
            var student = methodStudent.Result!;

            var methodCourse = await GetCourseResultAsync(student.Id, cancellationToken);
            if (!methodCourse.IsOK)
            {
                methodResult.AddErrorBadRequest(methodCourse.ErrorMessages);
                return methodResult;
            }
            var courseResult = methodCourse.Result!;

            overallHome.ProgressPercent = await GetCurrentProgressPercentAsync(courseResult, cancellationToken);

            var unitResult = await _unitResultRepository.ReadQueryable.Include(x => x.Unit)
                .Where(x => x.StudentId == student.Id && x.CourseResultId == courseResult.Id)
                .OrderBy(x => x.Status == EnumResultStatus.Process ? ValueOrderIndex.OrderIndexProcess :
                                      x.Status == EnumResultStatus.New ? ValueOrderIndex.OrderIndexNew :
                                      x.Status == EnumResultStatus.Done ? ValueOrderIndex.OrderIndexDone : ValueOrderIndex.OrderIndexOther)
                .ThenByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (unitResult == null)
            {
                methodResult.Result = overallHome;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            overallHome.Name = unitResult.Unit?.Name;
            overallHome.ProgressUnitPercent = await GetUnitCurrentProgressPercentAsync(unitResult, student.Id, cancellationToken);
            methodResult.Result = overallHome;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
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

        private async Task<double> GetCurrentProgressPercentAsync(CourseResult courseResult, CancellationToken cancellationToken)
        {
            var course = await _courseRepository.ReadQueryable
                .Include(x => x.CourseModules)
                .Where(x => x.Id == courseResult.CourseId)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (course == null)
            {
                return default;
            }

            var originalUnitIds = course.CourseModules
                .Where(x => x.CourseConfigType == EnumCourseConfigType.Unit)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            var units = await _unitRepository.ReadQueryable
                .Include(x => x.UnitModules)
                .Where(x => originalUnitIds.Contains(x.OriginalId) && x.VersionStatus == EnumVersionStatus.LastVersion)
                .ToListAsync(cancellationToken);

            var originalLessonIds = units.SelectMany(x => x.UnitModules)
                .Where(x => x.UnitConfigType == EnumUnitConfigType.Lesson)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            var lessons = await _lessonRepository.ReadQueryable
                .Include(x => x.LessonModules)
                .Where(x => originalLessonIds.Contains(x.OriginalId) && x.VersionStatus == EnumVersionStatus.LastVersion)
                .ToListAsync(cancellationToken);

            var totalModules = course.CourseModules.Count(x => x.CourseConfigType == EnumCourseConfigType.Test)
                 + units.SelectMany(x => x.UnitModules).Count(x => x.UnitConfigType == EnumUnitConfigType.Test)
                 + lessons.SelectMany(x => x.LessonModules).Count();

            var numberOfTestDone = await _testGroupResultRepository.ReadQueryable
                .Where(x => x.CourseResultId == courseResult.Id && x.Status == EnumResultStatus.Done)
                .CountAsync(cancellationToken);

            var studentId = courseResult.StudentId;

            var learnedLessionResultIds = await _lessonResultRepository.ReadQueryable
                .Where(x => x.CourseResultId == courseResult.Id && (x.Status == EnumResultStatus.Done || x.Status == EnumResultStatus.Process))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            var numberOfHomeworkDone = await _homeWorkResultRepository.ReadQueryable
                .Where(x => x.StudentId == studentId && learnedLessionResultIds.Contains(x.LessonResultId) && x.Status == EnumResultStatus.Done)
                .CountAsync(cancellationToken);

            var numberOfDocumentDone = await _documentResultRepository.ReadQueryable
                .Where(x => x.StudentId == studentId && learnedLessionResultIds.Contains(x.LessonResultId) && x.Status == EnumResultStatus.Done)
                .CountAsync(cancellationToken);

            var numberOfClassForumDone = await _classForumResultRepository.ReadQueryable
                .Where(x => x.StudentId == studentId && learnedLessionResultIds.Contains(x.LessonResultId) && x.ResultStatus == EnumResultStatus.Done)
                .CountAsync(cancellationToken);

            var numberOfVideoDone = await _videoResultRepository.ReadQueryable
                .Where(x => x.StudentId == studentId && learnedLessionResultIds.Contains(x.LessonResultId) && x.Status == EnumResultStatus.Done)
                .CountAsync(cancellationToken);

            return NumberHelper.GetPercent(
                numberOfTestDone
                + numberOfHomeworkDone
                + numberOfDocumentDone
                + numberOfClassForumDone
                + numberOfVideoDone,
                totalModules);
        }

        private async Task<double> GetUnitCurrentProgressPercentAsync(UnitResult unitResult, Guid studentId, CancellationToken cancellationToken)
        {
            var unit = await _unitRepository.ReadQueryable
                .Include(x => x.UnitModules)
                .Where(x => x.Id == unitResult.UnitId && x.VersionStatus == EnumVersionStatus.LastVersion)
                .ToListAsync(cancellationToken);

            var originalLessonIds = unit.SelectMany(x => x.UnitModules)
                .Where(x => x.UnitConfigType == EnumUnitConfigType.Lesson)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            var lessons = await _lessonRepository.ReadQueryable
                .Include(x => x.LessonModules)
                .Where(x => originalLessonIds.Contains(x.OriginalId) && x.VersionStatus == EnumVersionStatus.LastVersion)
                .ToListAsync(cancellationToken);

            var totalModulesOfUnit = unit.SelectMany(x => x.UnitModules).Count(x => x.UnitConfigType == EnumUnitConfigType.Test) + lessons.SelectMany(x => x.LessonModules).Count();

            var numberOfTestDone = await _testGroupResultRepository.ReadQueryable
                .Where(x => x.UnitResultId == unitResult.Id && x.StudentId == studentId && x.Status == EnumResultStatus.Done)
                .CountAsync(cancellationToken);

            var learnedLessionResultIds = await _lessonResultRepository.ReadQueryable
                .Where(x => x.UnitResultId == unitResult.Id && x.StudentId == studentId && (x.Status == EnumResultStatus.Done || x.Status == EnumResultStatus.Process))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            var numberOfHomeworkDone = await _homeWorkResultRepository.ReadQueryable
                .Where(x => x.StudentId == studentId && learnedLessionResultIds.Contains(x.LessonResultId) && x.Status == EnumResultStatus.Done)
                .CountAsync(cancellationToken);

            var numberOfDocumentDone = await _documentResultRepository.ReadQueryable
                .Where(x => x.StudentId == studentId && learnedLessionResultIds.Contains(x.LessonResultId) && x.Status == EnumResultStatus.Done)
                .CountAsync(cancellationToken);

            var numberOfClassForumDone = await _classForumResultRepository.ReadQueryable
                .Where(x => x.StudentId == studentId && learnedLessionResultIds.Contains(x.LessonResultId) && x.ResultStatus == EnumResultStatus.Done)
                .CountAsync(cancellationToken);

            var numberOfVideoDone = await _videoResultRepository.ReadQueryable
                .Where(x => x.StudentId == studentId && learnedLessionResultIds.Contains(x.LessonResultId) && x.Status == EnumResultStatus.Done)
                .CountAsync(cancellationToken);

            return NumberHelper.GetPercent(
                numberOfTestDone
                + numberOfHomeworkDone
                + numberOfDocumentDone
                + numberOfClassForumDone
                + numberOfVideoDone,
                totalModulesOfUnit);
        }
    }
}
