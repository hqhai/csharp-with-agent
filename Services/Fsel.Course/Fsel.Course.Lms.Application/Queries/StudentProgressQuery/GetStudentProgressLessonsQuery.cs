// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using System.Collections.Generic;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
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
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IMockTestSectionRepository _mockTestSectionRepository;
        private readonly IMockTestScoreRepository _mockTestScoreRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;

        public GetStudentProgressLessonsQueryHandler(ICourseRepository courseRepository, ISectionGroupRepository sectionGroupRepository, IMockTestSectionRepository mockTestSectionRepository, IMockTestScoreRepository mockTestScoreRepository, IMockTestResultRepository mockTestResultRepository, IMockTestRepository mockTestRepository, ILessonResultRepository lessonResultRepository, IUnitRepository unitRepository, IUserService userService, ISystemService systemService)
        {
            _courseRepository = courseRepository;
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
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { request.StudentId });
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }

            var student = studentResults?.Content?.Result?.FirstOrDefault();
            var studentId = student?.Id;
            var userId = student?.Human?.UserId;
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var unit = new Domain.Entities.Unit();
            if (course.CourseType == EnumCourseType.Academic)
            {
                unit = await _unitRepository.Queryable.Include(x => x.UnitLessons).FirstOrDefaultAsync(x => x.Id == request.UnitId, cancellationToken);
            }
            else
            {
                unit = await _unitRepository.Queryable.Include(x => x.UnitLessons).Include(x => x.UnitSkillMockTests).FirstOrDefaultAsync(x => x.Id == request.UnitId, cancellationToken);
            }
            if (unit == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var lessonIds = unit.UnitLessons.OrderBy(x => x.CreatedDate).Select(x => x.LessonId).ToList();
            var featureAccessTimeQuerys = lessonIds.Select(x => new FeatureAccessTimeQueryModel
            {
                CourseId = request.CourseId,
                UnitId = request.UnitId,
                LessonId = x,
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
            foreach (var item in lessonIds)
            {
                var lessonProgress = await GetLesson(item, studentId);
                var featureAccessTime = featureAccessTimes?.FirstOrDefault(x => x.LessonId == item);
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
                var mockTestResult = await _mockTestResultRepository.Queryable.Where(x => x.MockTestId == mockTestId && x.StudentId == studentId).FirstOrDefaultAsync(cancellationToken);
                var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel { CourseId = request.CourseId, ObjectId = mockTestId, UserId = userId ?? default, EnumFeature = EnumFeature.MockTest });
                var featureAccessTimeTest = featureAccessTimeResult?.Content?.Result;
                listLessonProgress.Add(await GetMockTest(request, mockTestId, featureAccessTimeTest));
            }

            methodResult.Result = listLessonProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<LessonStudentProgressModel> GetLesson(Guid? lessonId, Guid? studentId)
        {
            LessonStudentProgressModel lessonProgress = new LessonStudentProgressModel();
            var counts = new List<int>();
            var lessonResult = await _lessonResultRepository.GetAsync(lessonId, studentId);
            lessonProgress.Type = nameof(lessonResult.Lesson);
            if (lessonResult != null)
            {
                var lesson = lessonResult.Lesson;
                counts.Add(lessonResult.VideoResult?.Status == EnumResultStatus.Done ? 1 : 0);
                counts.Add(lessonResult.ClassForumResults.Where(x => x != null && (x.Status == EnumClassForumResultStatus.PendingForGrading || x.Status == EnumClassForumResultStatus.Graded) && x.StudentId == studentId).Count());
                counts.Add(lessonResult.HomeWorkResults.Where(x => x != null && x.Status == EnumResultStatus.Done && x.StudentId == studentId).GroupBy(x => x.LessonResultId).Count());
                if (lesson != null)
                {
                    lessonProgress.ObjectId = lesson.Id;
                    lessonProgress.Name = lesson.Name;
                }
                lessonProgress.Status = lessonResult.Status;
                lessonProgress.Percent = NumberHelper.ConvertPercentDouble(counts.Average());
                lessonProgress.ContentCompleted = string.Format("{0} / {1}", counts.Sum(), counts.Count);
            }
            return lessonProgress;
        }

        private async Task<LessonStudentProgressModel> GetMockTest(GetStudentProgressLessonsQuery request, Guid mockTestId, FeatureAccessTimeModel? featureAccessTime)
        {
            LessonStudentProgressModel mockTestProgress = new LessonStudentProgressModel();
            var mockTest = await _mockTestRepository.Queryable.Include(x => x.MockTestResults.Where(x => x.MockTestId == mockTestId && x.CourseId == request.CourseId && x.StudentId == request.StudentId && x.UnitId == request.UnitId))
                        .FirstOrDefaultAsync(x => x.Id == mockTestId);
            if (mockTest != null)
            {
                var query = from baseQ in _mockTestRepository.Queryable
                            join msg in _mockTestSectionRepository.Queryable on baseQ.Id equals msg.MockTestId
                            join sg in _sectionGroupRepository.Queryable on msg.SectionGroupId equals sg.Id
                            join mr in _mockTestResultRepository.Queryable on baseQ.Id equals mr.MockTestId into mrGroupG
                            from mrGroup in mrGroupG.DefaultIfEmpty()
                            join ms in _mockTestScoreRepository.Queryable on mrGroup.Id equals ms.MockTestResultId into msGroupG
                            from msGroup in msGroupG.DefaultIfEmpty()
                            where baseQ.Id == mockTestId && mrGroup.StudentId == request.StudentId
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
                        Percent = skillScore?.Percent ?? default,
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
                var mockTestResult = mockTest.MockTestResults.FirstOrDefault(x => x.MockTestId == mockTestId && x.CourseId == request.CourseId && x.StudentId == request.StudentId && x.UnitId == request.UnitId);
                mockTestProgress.Type = nameof(mockTestResult.MockTest);
                mockTestProgress.ObjectId = mockTest.Id;
                mockTestProgress.Name = mockTest.Name;

                if (mockTestResult != null)
                {
                    mockTestProgress.Status = mockTestResult.Status;
                    mockTestProgress.CorrectPercent = mockTestResult.Percent;
                    var isDone = mockTestResult.Status == EnumResultStatus.Done;
                    mockTestProgress.ContentCompleted = string.Format("{0} / {1}", isDone ? 1 : 0, 1);
                    mockTestProgress.Percent = isDone ? 100 : 0;
                    if (featureAccessTime != null)
                    {
                        mockTestProgress.TimeSpent = featureAccessTime.AccessTime;
                        mockTestProgress.LastVisited = featureAccessTime.LastVisited ?? null;
                    }
                }
            }
            return mockTestProgress;
        }
    }
}
