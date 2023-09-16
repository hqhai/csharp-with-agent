// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManageProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ManageProgress;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchManageStudentProgressQuery : SearchManageStudentProgressQueryModel, IRequest<MethodResult<PagingItemsModel<ManageStudentProgressModel>>>
    {
    }

    public class SearchManageStudentProgressQueryHandler : IRequestHandler<SearchManageStudentProgressQuery, MethodResult<PagingItemsModel<ManageStudentProgressModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUserService _userService;

        public SearchManageStudentProgressQueryHandler(ICourseRepository courseRepository,
            ILessonResultRepository lessonResultRepository,
            IUnitRepository unitRepository,
            ICourseResultRepository courseResultRepository,
            IUserService userService)
        {
            _courseRepository = courseRepository;
            _lessonResultRepository = lessonResultRepository;
            _unitRepository = unitRepository;
            _courseResultRepository = courseResultRepository;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<ManageStudentProgressModel>>> Handle(SearchManageStudentProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ManageStudentProgressModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            // Bước 1: Lấy danh sách CourseResults và Courses, đã sắp xếp theo CreatedDate.
            var sortedResults = await _courseResultRepository.Queryable
                .Where(cr => !cr.IsDeleted)
                .OrderBy(cr => cr.CreatedDate)
                .ToListAsync(cancellationToken);

            var sortedCourses = await _courseRepository.Queryable
                .Where(c => !c.IsDeleted)
                .ToListAsync(cancellationToken);

            // Bước 2: Nhóm danh sách đã sắp xếp theo StudentId, CourseId và CourseType.
            var courseResults = sortedResults
                .GroupBy(r => new { r.StudentId, r.CourseId })
                .Select(group => new
                {
                    StudentId = group.Key.StudentId,
                    CourseId = group.Key.CourseId,
                    CourseType = sortedCourses.FirstOrDefault(c => c.Id == group.Key.CourseId)?.CourseType ?? default, // Lấy CourseType từ danh sách đã sắp xếp.
                    MaxCreatedDate = group.Max(r => r.CreatedDate)
                })
                .OrderByDescending(x => x.MaxCreatedDate)
                .ToList();

            //var courseResults = query.AsEnumerable().ToList();

            //var courseResults = await query.ToListAsync(cancellationToken);
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(courseResults.Select(x => x.StudentId).ToList());
            var students = studentResults.Content?.Result;
            var manageStudents = new List<ManageStudentProgressModel>();
            foreach (var courseResult in courseResults)
            {
                ManageStudentProgressModel manageStudentProgressModel = new ManageStudentProgressModel();
                if (students != null && students.Any())
                {
                    var student = students.FirstOrDefault(x => x.Id == courseResult.StudentId);
                    manageStudentProgressModel.StudentId = courseResult.StudentId;
                    manageStudentProgressModel.FullName = student?.Human?.FullName;
                }
                manageStudentProgressModel.CourseType = courseResult.CourseType;
                manageStudentProgressModel.CourseId = courseResult.CourseId;
                var (currentProgress, progress, displayOrderUnit, displayOrderLesson) = await GetContentCompleted(courseResult.CourseId, courseResult.CourseType, courseResult.StudentId);
                manageStudentProgressModel.DisplayOrderLesson = displayOrderLesson;
                manageStudentProgressModel.DisplayOrderUnit = displayOrderUnit;
                manageStudentProgressModel.ContentProgress = string.Format("{0} / {1}", currentProgress, progress);
                manageStudents.Add(manageStudentProgressModel);
            }
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                manageStudents = manageStudents.Where(m => (m.FullName ?? string.Empty).Contains(request.Keyword)).ToList();
            }
            int totalItem = manageStudents.Count;
            var lists = manageStudents.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public async Task<(double, double, int, int)> GetContentCompleted(Guid courseId, EnumCourseType courseType, Guid? studentId)
        {
            if (courseType == EnumCourseType.Academic)
            {
                return await GetCourseAcademic(courseId, studentId);
            }
            else
            {
                return await GetCourseIELST(courseId, studentId);
            }
        }

        private async Task<(double, double, int, int)> GetCourseIELST(Guid courseId, Guid? studentId)
        {
            var course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests)
                                                         .ThenInclude(x => x.MockTest)
                                                         .ThenInclude(x => x!.MockTestResults.Where(x => x.StudentId == studentId && x.CourseId == courseId))
                                                         .FirstOrDefaultAsync(x => x.Id == courseId);
            var courseUnitMockTests = course?.CourseUnitMockTests.ToList();
            var unitIds = courseUnitMockTests?.Where(x => x.UnitId != null).Select(x => x.UnitId ?? default).ToList();
            var (currentProgress, progress, displayOrderUnit, displayOrderLesson) = await GetDisplayOrder(unitIds, studentId, courseUnitMockTests);
            var fullMockTest = courseUnitMockTests?.Select(x => x.MockTest).ToList();
            var fullMockTestResults = fullMockTest?.SelectMany(x => x!.MockTestResults).Where(x => x.StudentId == studentId && x.CourseId == courseId).ToList();
            var count = fullMockTestResults?.Where(x => x.Status == EnumResultStatus.Done).Count() ?? default;
            return (currentProgress + count, progress + count, displayOrderUnit, displayOrderLesson);
        }

        private async Task<(double, double, int, int)> GetCourseAcademic(Guid courseId, Guid? studentId)
        {
            var course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests)
                                                                          .ThenInclude(x => x.FinalTest)
                                                                          .ThenInclude(x => x!.FinalTestResults.Where(x => x.StudentId == studentId && x.CourseId == courseId))
                                                                          .FirstOrDefaultAsync(x => x.Id == courseId);
            var courseUnitMockTests = course?.CourseUnitMockTests.ToList();
            var unitIds = courseUnitMockTests?.Where(x => x.UnitId != null).Select(x => x.UnitId ?? default).ToList();
            var (currentProgress, progress, displayOrderUnit, displayOrderLesson) = await GetDisplayOrder(unitIds, studentId, courseUnitMockTests);
            var finalTest = courseUnitMockTests?.Select(x => x.FinalTest).ToList();
            var finalTestResult = finalTest?.SelectMany(x => x!.FinalTestResults).FirstOrDefault(x => x.StudentId == studentId && x.CourseId == courseId);
            var count = finalTestResult?.Status == EnumResultStatus.Done ? 1 : default;
            return (currentProgress + count, progress + count, displayOrderUnit + count, displayOrderLesson);
        }

        public async Task<(double, double, int, int)> GetDisplayOrder(IList<Guid>? unitIds, Guid? studentId, IList<CourseUnitMockTest>? courseUnitMockTests)
        {
            if (unitIds == null || !unitIds.Any() || courseUnitMockTests == null || !courseUnitMockTests.Any())
            {
                return (0, 0, 0, 0);
            }
            var displayOrderLesson = 0;
            var displayOrderUnit = 0;
            var units = await _unitRepository.GetsByIds(unitIds, studentId);
            if (units != null && units.Any())
            {
                var unitResult = units.SelectMany(x => x.UnitResults).OrderBy(x => x.CreatedDate).FirstOrDefault(x => x.Status != EnumResultStatus.Unfinished);
                displayOrderUnit = courseUnitMockTests.FirstOrDefault(x => x.UnitId == unitResult?.UnitId)?.DisplayOrder ?? default;
                var unitContents = courseUnitMockTests.Where(x => x.DisplayOrder <= displayOrderUnit && x.UnitId != null).Select(x => x.UnitId ?? default).ToList();
                if (unitResult != null)
                {
                    var lessonResult = await _lessonResultRepository.Queryable.OrderBy(x => x.CreatedDate).Where(x => x.UnitId == unitResult.UnitId && x.StudentId == studentId && x.Status != EnumResultStatus.Unfinished).FirstOrDefaultAsync();
                    if (lessonResult != null)
                    {
                        displayOrderLesson = units.Where(x => unitContents.Contains(x.Id)).SelectMany(x => x.UnitLessons).Where(x => unitContents.Contains(x.UnitId) && x.LessonId == lessonResult.LessonId).Count();
                    }
                }
            }
            var (currentProgress, progress) = await GetContentComplete(units, studentId);
            return (currentProgress, progress, displayOrderUnit, displayOrderLesson);
        }

        public async Task<(int, int)> GetContentComplete(IList<Domain.Entities.Unit>? units, Guid? studentId)
        {
            var countVideo = 0;
            var countHomeWork = 0;
            var countClassForum = 0;
            var lessonIds = new List<Guid>();
            if (units != null && units.Any())
            {
                lessonIds = units.SelectMany(x => x.UnitLessons).Select(x => x.Lesson).Select(x => x!.Id).ToList();
                var lessonResultIds = units.SelectMany(x => x.LessonResults).Where(x => lessonIds.Contains(x.LessonId) && x.StudentId == studentId).Select(x => x.Id).ToList();
                var lessonResults = await _lessonResultRepository.GetsByIds(lessonResultIds);
                if (lessonResults != null && lessonResults.Any())
                {
                    countVideo = lessonResults.Select(x => x.VideoResult).Where(x => x != null && x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).Count();
                    countHomeWork = lessonResults.SelectMany(x => x.ClassForumResults).Where(x => x != null && x.Status == EnumClassForumResultStatus.Graded && lessonResultIds.Contains(x.LessonResultId)).Count();
                    countClassForum = lessonResults.SelectMany(x => x.HomeWorkResults).Where(x => x != null && x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).Count();
                }
            }
            return (lessonIds.Count * 3, countVideo + countClassForum + countHomeWork);
        }
    }
}
