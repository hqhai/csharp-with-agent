// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using System.Linq.Dynamic.Core;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProgressUnitsQuery : IRequest<MethodResult<IList<UnitStudentProgressModel>>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class GetStudentProgressUnitsQueryHandler : IRequestHandler<GetStudentProgressUnitsQuery, MethodResult<IList<UnitStudentProgressModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IAggregateResultQueryService _aggregateResultQueryService;

        public GetStudentProgressUnitsQueryHandler(ICourseRepository courseRepository, IUnitRepository unitRepository, ManagerProgressHelper managerProgressHelper, IUserService userService, ISystemService systemService, ICourseUnitMockTestRepository courseUnitMockTestRepository, ICourseResultRepository courseResultRepository, IAggregateResultQueryService aggregateResultQueryService)
        {
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _managerProgressHelper = managerProgressHelper;
            _userService = userService;
            _systemService = systemService;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _courseResultRepository = courseResultRepository;
            _aggregateResultQueryService = aggregateResultQueryService;
        }

        public async Task<MethodResult<IList<UnitStudentProgressModel>>> Handle(GetStudentProgressUnitsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<UnitStudentProgressModel>> methodResult = new MethodResult<IList<UnitStudentProgressModel>>();
            IList<UnitStudentProgressModel> unitStudentProgress = new List<UnitStudentProgressModel>();

            var studentResults = await _userService.GetUserByStudentIdWithCache(request.StudentId);
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }
            var student = studentResults?.Content?.Result;

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var userId = student.UserId;

            var course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests).FirstOrDefaultAsync(x => x.Id == request.CourseId, cancellationToken);

            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(p => p.CourseId == course.Id && p.StudentId == student.Id, cancellationToken);

            if (courseResult == null)
            {
                return methodResult;
            }

            var learningTree = await _aggregateResultQueryService.GetLearningTreeFromCourseToLesson(
            student.Id,
            courseResult.Id,
            cancellationToken);

            var units = learningTree
               .GetAllItemByType<UnitComponent>()
               .ToList();

            var lessons = learningTree
                .GetAllItemByType<LessonComponent>()
                .ToList();

            var courseUnitMockTests = course.CourseUnitMockTests.OrderBy(x => x.DisplayOrder).ToList();
            var featureAccessTimeTest = new List<FeatureAccessTimeModel>();
            var unitIds = courseUnitMockTests.Where(x => x.UnitId.HasValue).Select(x => x.UnitId!.Value).ToList();
            var featureAccessTimes = await _systemService.GetFeatureAccessTimesAsync(new FeatureAccessTimesQueryModel
            {
                UserId = userId,
                FeatureAccessTimes = unitIds.Select(x => new FeatureAccessTimeQueryModel
                {
                    UserId = userId,
                    UnitId = x,
                    CourseId = course.Id
                }).ToList(),
            });
            var featureAccessTimeUnit = featureAccessTimes?.Content?.Result;
            foreach (var courseUnit in courseUnitMockTests)
            {
                if (!courseUnit.UnitId.HasValue)
                {
                    continue;
                }
                var unitProgress = await _managerProgressHelper.GetUnitManager(course.Id, courseUnit.UnitId.Value, student.Id);
                if (unitProgress == null)
                {
                    continue;
                }
                var featureAccessTime = featureAccessTimeUnit?.FirstOrDefault(x => x.UnitId == courseUnit.UnitId);
                if (featureAccessTime != null)
                {
                    unitProgress.TimeSpent = featureAccessTime.AccessTime;
                    unitProgress.LastVisited = featureAccessTime.LastVisited ?? null;
                }
                unitStudentProgress.Add(unitProgress);
            }

            methodResult.Result = unitStudentProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
