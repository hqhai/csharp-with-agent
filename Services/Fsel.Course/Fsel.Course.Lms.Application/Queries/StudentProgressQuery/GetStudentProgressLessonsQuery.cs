// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using System.Collections.Generic;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProgressLessonsQuery : IRequest<MethodResult<IList<LessonStudentProgressModel>>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
    }

    public class GetStudentProgressLessonsQueryHandler : IRequestHandler<GetStudentProgressLessonsQuery, MethodResult<IList<LessonStudentProgressModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IMockTestSectionRepository _mockTestSectionRepository;
        private readonly IMockTestScoreRepository _mockTestScoreRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private const int MaxModuleLesson = 3;

        public GetStudentProgressLessonsQueryHandler(ICourseRepository courseRepository, ManagerProgressHelper managerProgressHelper, ISectionGroupRepository sectionGroupRepository, IMockTestSectionRepository mockTestSectionRepository, IMockTestScoreRepository mockTestScoreRepository, IMockTestResultRepository mockTestResultRepository, IMockTestRepository mockTestRepository, ILessonResultRepository lessonResultRepository, IUnitRepository unitRepository, IUserService userService, ISystemService systemService)
        {
            _courseRepository = courseRepository;
            _managerProgressHelper = managerProgressHelper;
            _sectionGroupRepository = sectionGroupRepository;
            _mockTestSectionRepository = mockTestSectionRepository;
            _mockTestScoreRepository = mockTestScoreRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestRepository = mockTestRepository;
            _lessonResultRepository = lessonResultRepository;
            _unitRepository = unitRepository;
            _userService = userService;
            _systemService = systemService;
        }

        public async Task<MethodResult<IList<LessonStudentProgressModel>>> Handle(GetStudentProgressLessonsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<LessonStudentProgressModel>> methodResult = new MethodResult<IList<LessonStudentProgressModel>>();
            IList<LessonStudentProgressModel> listLessonProgress = new List<LessonStudentProgressModel>();
            var studentResults = await _userService.GetUserByStudentId(request.StudentId);
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
            var studentId = student.Id;
            var userId = student.Human?.UserId;
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var unit = new Domain.Entities.Unit();
            if (course.CourseType == EnumCourseType.Academic)
            {
                unit = await _unitRepository.Queryable.Include(x => x.UnitLessons).ThenInclude(x => x.Lesson)
                    .FirstOrDefaultAsync(x => x.Id == request.UnitId, cancellationToken);
            }
            else
            {
                unit = await _unitRepository.Queryable.Include(x => x.UnitLessons).ThenInclude(x => x.Lesson)
                    .Include(x => x.UnitSkillMockTests).FirstOrDefaultAsync(x => x.Id == request.UnitId, cancellationToken);
            }
            if (unit == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var lessons = unit.UnitLessons.OrderBy(x => x.CreatedDate).Where(x => x.Lesson != null).Select(x => x.Lesson!).ToList();
            var featureAccessTimeQuerys = lessons.Select(x => new FeatureAccessTimeQueryModel
            {
                CourseId = request.CourseId,
                UnitId = request.UnitId,
                LessonId = x.Id,
                UserId = userId ?? default
            }).ToList();
            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimesAsync(new FeatureAccessTimesQueryModel
            {
                FeatureAccessTimes = featureAccessTimeQuerys,
                UserId = userId ?? default
            });
            if (!featureAccessTimeResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(featureAccessTimeResults));
                return methodResult;
            }
            var featureAccessTimes = featureAccessTimeResults.Content?.Result;
            foreach (var lesson in lessons)
            {
                if (lesson == null)
                {
                    continue;
                }
                var lessonProgress = await GetLessonAsync(request, lesson);

                var featureAccessTime = featureAccessTimes?.FirstOrDefault(x => x.LessonId == lesson.Id);
                lessonProgress.Type = nameof(Lesson);
                if (featureAccessTime != null)
                {
                    lessonProgress.TimeSpent = featureAccessTime.AccessTime;
                    lessonProgress.LastVisited = featureAccessTime.LastVisited ?? null;
                    lessonProgress.Visit = featureAccessTime.Visit;
                }
                listLessonProgress.Add(lessonProgress);
            }
            if (unit.UnitSkillMockTests.Any())
            {
                var mockTestId = unit.UnitSkillMockTests.Select(x => x.MockTestId).FirstOrDefault();
                var mockTestResult = await _mockTestResultRepository.Queryable.Where(x => x.MockTestId == mockTestId && x.CourseId == request.CourseId)
                                                                              .FirstOrDefaultAsync(x => x.StudentId == request.StudentId && x.UnitId == request.UnitId, cancellationToken);
                if (mockTestResult != null)
                {
                    var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel
                    {
                        CourseId = request.CourseId,
                        UnitId = request.UnitId,
                        ObjectId = mockTestResult.Id,
                        UserId = userId ?? default,
                        EnumFeature = EnumFeature.MockTest
                    });
                    var featureAccessTimeTest = featureAccessTimeResult?.Content?.Result;
                    var mockTestProgess = await GetProgressSkillMockTestAsync(mockTestResult, featureAccessTimeTest);
                    if (mockTestProgess != null)
                    {
                        listLessonProgress.Add(mockTestProgess);
                    }
                }
            }

            methodResult.Result = listLessonProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<LessonStudentProgressModel> GetLessonAsync(GetStudentProgressLessonsQuery request, Lesson lesson)
        {
            LessonStudentProgressModel lessonProgress = new LessonStudentProgressModel();
            var lessonResult = await _lessonResultRepository.Queryable
                                    .Where(x => x.CourseId == request.CourseId && x.UnitId == request.UnitId)
                                    .FirstOrDefaultAsync(x => x.LessonId == lesson.Id && x.StudentId == request.StudentId);

            lessonProgress.ObjectId = lesson.Id;
            lessonProgress.Name = lesson.Name;
            if (lessonResult == null)
            {
                return lessonProgress;
            }
            var query = await _lessonResultRepository.Queryable.Include(x => x.VideoResults).Include(x => x.ClassForumResults).Include(x => x.HomeWorkResults)
                                 .Where(x => x.Id == lessonResult.Id)
                                 .Select(x => new
                                 {
                                     CountVideo = x.VideoResult != null && x.VideoResult.Status == EnumResultStatus.Done ? 1 : 0,
                                     CountClassForum = x.ClassForumResults.Any() && x.ClassForumResults.All(x => x.Status.HasValue) ? 1 : 0,
                                     CountHomeWork = x.HomeWorkResults.Any() && x.HomeWorkResults.All(x => x.Status == EnumResultStatus.Done) ? 1 : 0
                                 }).ToListAsync();
            var completeLesson = query.Sum(x => x.CountVideo + x.CountClassForum + x.CountHomeWork);

            lessonProgress.Id = lessonResult.Id;
            lessonProgress.Status = lessonResult.Status;
            lessonProgress.Percent = NumberHelper.GetPercent(completeLesson, MaxModuleLesson);
            lessonProgress.ContentCompleted = string.Format("{0} / {1}", completeLesson, MaxModuleLesson);
            return lessonProgress;
        }

        private async Task<LessonStudentProgressModel?> GetProgressSkillMockTestAsync(MockTestResult mockTestResult, FeatureAccessTimeModel? featureAccessTime)
        {
            var mockTest = await _mockTestRepository.GetByIdAsync(mockTestResult.MockTestId);
            if (mockTest == null)
            {
                return default;
            }
            var isDone = mockTestResult.Status == EnumResultStatus.Done;
            LessonStudentProgressModel mockTestProgress = new LessonStudentProgressModel
            {
                Id = mockTestResult.Id,
                Type = nameof(mockTestResult.MockTest),
                ObjectId = mockTest.Id,
                Name = mockTest.Name,
                Status = mockTestResult.Status,
                CorrectPercent = mockTestResult.Percent,
                Percent = isDone ? 100 : 0,
                TimeSpent = featureAccessTime?.AccessTime ?? default,
                LastVisited = featureAccessTime?.LastVisited ?? null,
                Visit = featureAccessTime?.Visit ?? default,
                ContentCompleted = string.Format("{0} / {1}", isDone ? 1 : 0, 1)
            };
            var query = from baseQ in _mockTestRepository.Queryable
                        join msg in _mockTestSectionRepository.Queryable on baseQ.Id equals msg.MockTestId
                        join sg in _sectionGroupRepository.Queryable on msg.SectionGroupId equals sg.Id
                        join mr in _mockTestResultRepository.Queryable on baseQ.Id equals mr.MockTestId into mrGroupG
                        from mrGroup in mrGroupG.DefaultIfEmpty()
                        join ms in _mockTestScoreRepository.Queryable on mrGroup.Id equals ms.MockTestResultId into msGroupG
                        from msGroup in msGroupG.DefaultIfEmpty()
                        where baseQ.Id == mockTestResult.MockTestId && mrGroup.Id == mockTestResult.Id
                        group new { sg, mrGroup, msGroup } by new { sg.CourseSkill } into g
                        select new
                        {
                            Skill = g.Key.CourseSkill,
                            MockTestResult = g.Select(x => x.mrGroup).FirstOrDefault(),
                            MockTestScores = g.Select(x => x.msGroup).ToList()
                        };
            var skillMockTest = await query.ToListAsync();
            mockTestProgress.TestSkillScores = skillMockTest.Select(x =>
            {
                var skillScore = x.MockTestResult?.SkillScores?.FirstOrDefault(z => z.Skill == x.Skill);
                var skillScores = new TestSkillScores
                {
                    Skill = x.Skill,
                    CorrectCount = skillScore?.CorrectCount ?? default,
                    TotalCount = skillScore?.TotalCount ?? default,
                    CountQuestion = skillScore?.CountQuestion ?? default,
                    TotalQuestion = skillScore?.TotalQuestion ?? default,
                    Scores = skillScore?.Scores ?? default,
                };
                if (x.Skill == EnumCourseSkill.Speaking || x.Skill == EnumCourseSkill.Writing)
                {
                    if (x.MockTestScores.Any() && x.MockTestScores.All(x => x != null))
                    {
                        skillScores.Status = EnumResultStatus.Done;
                    }
                    else
                    {
                        skillScores.Status = EnumResultStatus.Process;
                    }
                }
                return skillScores;
            }).FirstOrDefault();

            return mockTestProgress;
        }
    }
}
