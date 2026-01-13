// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentLearningReportQuery : IRequest<MethodResult<StudentCourseProgressModel>>
    {
        public Guid UserId { get; set; }
    }

    public class GetStudentLearningReportQueryHandler : IRequestHandler<GetStudentLearningReportQuery, MethodResult<StudentCourseProgressModel>>
    {
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IAggregateResultQueryService _aggregateResultQueryService;
        private readonly ICourseRepository _courseRepository;
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private const string CourseDone = "Đã hoàn thành khóa học";

        public GetStudentLearningReportQueryHandler(ManagerProgressHelper managerProgressHelper,
            ISystemService systemService,
            IUserService userService,
            ICourseResultRepository courseResultRepository,
            IAggregateResultQueryService aggregateResultQueryService,
            ICourseRepository courseRepository,
            ITestGroupResultRepository testGroupResultRepository)
        {
            _managerProgressHelper = managerProgressHelper;
            _systemService = systemService;
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _aggregateResultQueryService = aggregateResultQueryService;
            _courseRepository = courseRepository;
            _testGroupResultRepository = testGroupResultRepository;
        }

        public async Task<MethodResult<StudentCourseProgressModel>> Handle(GetStudentLearningReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentCourseProgressModel> methodResult = new MethodResult<StudentCourseProgressModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId);
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                return methodResult;
            }

            var courseResult = await _courseResultRepository.ReadQueryable.Where(x => x.StudentId == student.Id && x.WorkingStatus == EnumWorkingStatus.Active)
                                                            .FirstOrDefaultAsync(cancellationToken);

            var courseId = courseResult?.CourseId ?? student.CourseId;
            if (!courseId.HasValue)
            {
                return methodResult;
            }

            var course = await _courseRepository.ReadQueryable.Include(p => p.Program).ThenInclude(p => p.CategoryParent).Include(p => p.Level).FirstOrDefaultAsync(p => p.Id == courseId, cancellationToken);

            var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeByUserIdsAsync(new List<Guid> { request.UserId });
            var featureAccessTime = featureAccessTimeResult?.Content?.Result?.FirstOrDefault();

            var lastDate = featureAccessTime?.LastVisited;

            int totalLessons = 0;
            int currentLessonIndex = 0;
            string? currentUnit = string.Empty;

            if (courseResult != null)
            {
                var learningTree = await _aggregateResultQueryService.GetLearningTreeFromCourseToLesson(
                                                student.Id,
                                                courseResult.Id,
                                                cancellationToken);

                var lessons = learningTree
                    .GetAllItemByType<LessonComponent>()
                    .ToList();

                var units = learningTree
                    .GetAllItemByType<UnitComponent>()
                    .ToList();

                totalLessons = lessons.Count;
                currentLessonIndex = lessons.Count(n => n.Status == EnumResultStatus.Done);

                var unit = units.FirstOrDefault(p => p.Status == EnumResultStatus.Process);
                if (unit == null)
                {
                    unit = units.FirstOrDefault(p => p.Status == EnumResultStatus.New);
                }
                if (unit == null)
                {
                    unit = units.Where(p => p.Status == EnumResultStatus.Done).OrderByDescending(p => p.UpdatedDate).FirstOrDefault();
                }
                currentUnit = unit?.ComponentName;
            }

            var testGroupResults = await _testGroupResultRepository.ReadQueryable.Include(p => p.Category).ThenInclude(p => p.CategoryParent).Where(p => p.StudentId == student.Id && p.TestType == EnumTestType.PlacementTest && (p.Status == EnumResultStatus.Process || p.Status == EnumResultStatus.Done)).ToListAsync(cancellationToken);

            var subjects = testGroupResults.Where(p => p.Category != null && p.Category.CategoryParent != null).Select(p => new CategoryModel
            {
                Id = p.Category?.CategoryParent?.Id ?? default,
                Name = p.Category?.CategoryParent?.Name,
                Type = p.Category?.CategoryParent?.Type ?? default,
                Status = p.Category?.CategoryParent?.Status ?? default,
                ParentId = p.Category?.CategoryParent?.ParentId,
            }).ToList();

            if (course?.Program != null && course.Program.CategoryParent != null)
            {
                subjects.Add(new CategoryModel
                {
                    Id = course.Program?.CategoryParent?.Id ?? default,
                    Name = course.Program?.CategoryParent?.Name,
                    Type = course.Program?.CategoryParent?.Type ?? default,
                    Status = course.Program?.CategoryParent?.Status ?? default,
                    ParentId = course.Program?.CategoryParent?.ParentId,
                });
            }

            //var courseResults = await _courseResultRepository.ReadQueryable.Include(p => p.Course).ThenInclude(p => p.Program).ThenInclude(p => p.CategoryParent).Where(p => p.StudentId == student.Id).ToListAsync(cancellationToken);

            subjects = subjects.DistinctBy(p => p.Id).ToList();

            methodResult.Result = new StudentCourseProgressModel
            {
                StudentId = student.Id,
                CourseId = courseId.Value,
                CurrentLessonIndex = currentLessonIndex,
                TotalLessons = totalLessons,
                UnitName = courseResult?.Status == EnumResultStatus.Done ? CourseDone : currentUnit,
                IsCourseCompleted = courseResult?.Status == EnumResultStatus.Done,
                StartDate = courseResult != null && courseResult.ProcessDate.HasValue ? courseResult.ProcessDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam) : null,
                TotalActiveDuration = featureAccessTime?.AccessTime,
                LastAccessedDate = lastDate.HasValue ? lastDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam) : null,
                ProgramName = course?.Program?.Name,
                SubjectName = course?.Program?.CategoryParent?.Name,
                LevelName = course?.Level?.Name,
                CategoryModels = subjects,
            };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
