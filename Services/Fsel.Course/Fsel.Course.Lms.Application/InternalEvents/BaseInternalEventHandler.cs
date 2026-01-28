// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.OtherCmd;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using static Fsel.Shared.Constants.ValueSettings;

    public class BaseInternalEventHandler
    {
        protected readonly IVideoResultRepository _videoResultRepository;
        protected readonly IClassForumResultRepository _classForumResultRepository;
        protected readonly IUnitResultRepository _unitResultRepository;
        protected readonly ICourseResultRepository _courseResultRepository;
        protected readonly ICourseRepository _courseRepository;
        protected readonly IUnitRepository _unitRepository;
        protected readonly IFinalTestResultRepository _finalTestResultRepository;
        protected readonly IMockTestResultRepository _mockTestResultRepository;
        protected readonly IHomeWorkResultRepository _homeWorkResultRepository;
        protected readonly IUserService _userService;
        protected readonly ILogger<BaseInternalEventHandler> _logger;
        protected readonly SaveUserCourseSettingPublisher _saveUserCourseSettingPublisher;
        protected readonly IMediator _mediator;
        protected readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        protected readonly AppSetting _appSetting;
        protected readonly ISystemService _systemService;
        protected readonly IOrderService _orderService;
        private readonly ILessonNoteRepository _lessonNoteRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        protected readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly SendMailFinishCoursePublisher _sendMailFinishCoursePublisher;

        public BaseInternalEventHandler(ISystemService systemService, AppSetting appSetting,
            ICourseUnitMockTestRepository courseUnitMockTestRepository,
            IMediator mediator,
            IUserService userService,
            ILogger<BaseInternalEventHandler> logger,
            SaveUserCourseSettingPublisher saveUserCourseSettingPublisher,
            IVideoResultRepository videoResultRepository,
            IClassForumResultRepository classForumResultRepository,
            IUnitResultRepository unitResultRepository,
            ICourseResultRepository courseResultRepository,
            ICourseRepository courseRepository,
            IUnitRepository unitRepository,
            IFinalTestResultRepository finalTestResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            QuestBoardPublisher questBoardPublisher,
            IOrderService orderService,
            ILessonNoteRepository lessonNoteRepository,
            ILessonResultRepository lessonResultRepository,
            NotificationMessagePublisher notificationMessagePublisher, SendMailFinishCoursePublisher sendMailFinishCoursePublisher)
        {
            _videoResultRepository = videoResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _unitResultRepository = unitResultRepository;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _userService = userService;
            _logger = logger;
            _saveUserCourseSettingPublisher = saveUserCourseSettingPublisher;
            _mediator = mediator;
            _appSetting = appSetting;
            _systemService = systemService;
            _questBoardPublisher = questBoardPublisher;
            _orderService = orderService;
            _lessonNoteRepository = lessonNoteRepository;
            _lessonResultRepository = lessonResultRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
            _sendMailFinishCoursePublisher = sendMailFinishCoursePublisher;
        }

        private async Task<(List<SkillScores>, double)> GetCourseResult(Course course, IList<Guid> unitIds, Guid studentId, Guid? finalTestId)
        {
            ArgumentNullException.ThrowIfNull(unitIds);

            var units = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.StudentId == studentId && x.CourseId == course.Id)).Where(x => unitIds.Contains(x.Id)).ToListAsync();
            var lessonResultIds = units.SelectMany(x => x.LessonResults).Where(x => x.StudentId == studentId && x.CourseId == course.Id).Select(x => x.Id).ToList();

            var (videoSkillScores, percentVideo) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.Standalone, courseType: course.CourseType);
            var (homeWorkSkillScores, percentHomeWork) = await GetHomeWordsSkillScores(lessonResultIds, courseType: course.CourseType);
            var (classForumSkillScores, percentClassForum) = await GetClassForumSkillScores(lessonResultIds, courseType: course.CourseType);

            var percents = new List<double> { percentClassForum, percentHomeWork, percentVideo };
            List<SkillScores> mergedSkillScores = videoSkillScores.Concat(homeWorkSkillScores).Concat(classForumSkillScores).ToList();
            if (course.CourseType == EnumCourseType.Academic)
            {
                var (unitSkillScores, percentUnitSkill) = await GetSkillScoreByCourses(course.Id, unitIds, studentId, EnumTimeCodeType.UnitTest, OverallPercentCourse.OverallAcaPercentUnitTest);
                if (!unitSkillScores.Any())
                {
                    percentUnitSkill = OverallPercentCourse.OverallAcaPercentUnitTest;
                }
                var (skillSkillScores, percentSkill) = await GetSkillScoreByCourses(course.Id, unitIds, studentId, EnumTimeCodeType.SkillTest, OverallPercentCourse.OverallAcaPercentSkillTest);
                if (!skillSkillScores.Any())
                {
                    percentSkill = OverallPercentCourse.OverallAcaPercentSkillTest;
                }
                var (finalTestSkillScores, percentFinalTest) = await GetFinalTestSkillScore(finalTestId, course, studentId);
                mergedSkillScores = mergedSkillScores.Concat(skillSkillScores).Concat(unitSkillScores).Concat(finalTestSkillScores).ToList();
                percents.AddRange(new List<double> { percentUnitSkill, percentSkill, percentFinalTest });
            }
            if (course.CourseType == EnumCourseType.EnglishFoundation)
            {
                var (unitSkillScores, percentUnitSkill) = await GetSkillScoreByCourses(course.Id, unitIds, studentId, EnumTimeCodeType.UnitTest, OverallPercentCourse.OverallRFIPercentUnitTest);
                if (!unitSkillScores.Any())
                {
                    percentUnitSkill = OverallPercentCourse.OverallRFIPercentUnitTest;
                }
                var (finalTestSkillScores, percentFinalTest) = await GetFinalTestSkillScore(finalTestId, course, studentId);
                mergedSkillScores = mergedSkillScores.Concat(unitSkillScores).Concat(finalTestSkillScores).ToList();
                percents.AddRange(new List<double> { percentUnitSkill, percentFinalTest });
            }
            var groupedSkillScores = mergedSkillScores.GroupBy(x => x.Skill).Select(group => GetSumSkillScore(group)).OrderBy(x => x.Skill).ToList();
            return (groupedSkillScores, percents.Sum());
        }

        public async Task<(List<SkillScores>, double)> GetVideoSkillScores(IList<Guid>? lessonResultIds, EnumTimeCodeType type, int percentSkill = default, EnumCourseType? courseType = null)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            List<SkillScores> skillScores = new List<SkillScores>();
            var videoResults = await _videoResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
            if (videoResults != null && videoResults.Any())
            {
                skillScores = videoResults.Where(x => x.VideoSkillScores != null && x.VideoSkillScores.Any()).SelectMany(x => x.VideoSkillScores!)
                    .Where(x => x.Type == type && x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!).GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList();
            }

            if (courseType.HasValue && courseType == EnumCourseType.Ielts)
            {
                return (skillScores, skillScores.Any() ? NumberHelper.ConvertDoublePercent(skillScores.Sum(x =>
                {
                    if (x.Skill == EnumCourseSkill.Writing || x.Skill == EnumCourseSkill.Speaking)
                    {
                        return NumberHelper.ConvertRound(x.Percent * OverallPercentCourse.SkillSWIELTSPercentVideo);
                    }
                    return NumberHelper.ConvertRound(x.Percent * OverallPercentCourse.SkillIELTSPercentVideo);
                })) : default);
            }
            if (courseType.HasValue)
            {
                switch (courseType.Value)
                {
                    case EnumCourseType.Academic:
                        percentSkill = OverallPercentCourse.OverallAcaPercentVideo;
                        break;

                    case EnumCourseType.EnglishFoundation:
                        percentSkill = OverallPercentCourse.OverallRFIPercentVideo;
                        break;
                }
            }
            return (skillScores, GetDoublePercent(skillScores, percentSkill));
        }

        public async Task<(List<SkillScores>, double)> GetClassForumSkillScores(IList<Guid>? lessonResultIds, int percentSkill = default, EnumCourseType? courseType = null)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            List<SkillScores> skillScores = new List<SkillScores>();
            var classForumResults = await _classForumResultRepository.Queryable.Where(x => x.Status.HasValue).Where(x => lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
            if (classForumResults != null && classForumResults.Any())
            {
                skillScores = classForumResults.Where(x => x.SkillScores != null && x.SkillScores.Any())
                                               .SelectMany(x => x.SkillScores!)
                                               .GroupBy(x => x.Skill)
                                               .Select(x => GetSkillScore(x))
                                               .ToList();
            }
            if (courseType.HasValue)
            {
                switch (courseType.Value)
                {
                    case EnumCourseType.Academic:
                        percentSkill = OverallPercentCourse.OverallAcaPercentClassForum;
                        break;

                    case EnumCourseType.Ielts:
                        percentSkill = OverallPercentCourse.OverallIELTSPercentClassForum;
                        break;

                    case EnumCourseType.EnglishFoundation:
                        percentSkill = OverallPercentCourse.OverallRFIPercentClassForum;
                        break;
                }
            }
            return (skillScores, GetDoublePercent(skillScores, percentSkill));
        }

        public async Task<(List<SkillScores>, double)> GetHomeWordsSkillScores(IList<Guid>? lessonResultIds, int percentSkill = default, EnumCourseType? courseType = null)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            List<SkillScores> skillScores = new List<SkillScores>();
            var homeWorkResults = await _homeWorkResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
            if (homeWorkResults != null)
            {
                skillScores = homeWorkResults.Where(x => x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!).GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList();
            }
            if (courseType.HasValue)
            {
                switch (courseType.Value)
                {
                    case EnumCourseType.Academic:
                        percentSkill = OverallPercentCourse.OverallAcaPercentHomeWork;
                        break;

                    case EnumCourseType.Ielts:
                        percentSkill = OverallPercentCourse.OverallIELTSPercentHomeWork;
                        break;

                    case EnumCourseType.EnglishFoundation:
                        percentSkill = OverallPercentCourse.OverallRFIPercentHomeWork;
                        break;
                }
            }
            return (skillScores, GetDoublePercent(skillScores, percentSkill));
        }

        private static double GetDoublePercent(IList<SkillScores>? skillScores, int percentOccupy, int numberOfElements = default)
        {
            if (skillScores == null || !skillScores.Any())
            {
                return default;
            }
            if (numberOfElements != default)
            {
                return NumberHelper.ConvertDoublePercent(skillScores.Average(x => x.Percent * percentOccupy / numberOfElements));
            }
            else
            {
                return NumberHelper.ConvertDoublePercent(skillScores.Sum(x => x.Percent * percentOccupy / skillScores.Count));
            }
        }

        public static SkillScores GetSumSkillScore(IGrouping<EnumCourseSkill, SkillScores>? group)
        {
            if (group != null)
            {
                return new SkillScores
                {
                    Skill = group.Key,
                    Scores = group.Average(x => x.Scores),
                    TotalCount = group.Sum(x => x.TotalCount),
                    CorrectCount = group.Sum(x => x.CorrectCount),
                    CountQuestion = group.Sum(x => x.CountQuestion),
                    TotalQuestion = group.Sum(x => x.TotalQuestion),
                };
            }
            return new SkillScores();
        }

        public static SkillScores GetSkillScore(IGrouping<EnumCourseSkill, SkillScores>? x)
        {
            SkillScores skillScores = new SkillScores();
            if (x != null)
            {
                skillScores.Skill = x.Key;
                skillScores.TotalQuestion = x.Sum(x => x.TotalQuestion);
                skillScores.CountQuestion = x.Sum(x => x.CountQuestion);
                skillScores.TotalCount = x.Sum(x => x.TotalCount);
                skillScores.CorrectCount = x.Sum(x => x.CorrectCount);
                return skillScores;
            }
            ;
            return skillScores;
        }

        private async Task<(List<SkillScores>, double)> GetSkillScoreByCourses(Guid courseId, IList<Guid>? unitIds, Guid studentId, EnumTimeCodeType type, int percentSkill = default)
        {
            ArgumentNullException.ThrowIfNull(unitIds);
            var skillScorePercents = new List<(List<SkillScores>, double)>();
            var units = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.StudentId == studentId && x.CourseId == courseId && x.Status == EnumResultStatus.Done))
                                                      .Where(x => unitIds.Contains(x.Id))
                                                      .ToListAsync();
            units = units.OrderBy(x => unitIds.IndexOf(x.Id)).ToList();
            var listLessonResultId = units.SelectMany(x => x.LessonResults).Where(x => x.StudentId == studentId && x.CourseId == courseId && x.Status == EnumResultStatus.Done)
                                       .Select(x => x.Id).ToList();

            var videoResults = await _videoResultRepository.Queryable.Where(x => listLessonResultId.Contains(x.LessonResultId) && x.Status == EnumResultStatus.Done).ToListAsync();
            foreach (var unit in units)
            {
                var lessonResultIds = unit.LessonResults.Where(x => x.StudentId == studentId && x.CourseId == courseId && x.Status == EnumResultStatus.Done).Select(x => x.Id).ToList();
                if (lessonResultIds == null || !lessonResultIds.Any())
                {
                    continue;
                }
                var listVideoResults = videoResults.Where(x => lessonResultIds.Contains(x.LessonResultId)).ToList();
                var videoSkillScore = listVideoResults.Where(x => x.VideoSkillScores != null && x.VideoSkillScores.Any())
                                                      .SelectMany(x => x.VideoSkillScores!)
                                                      .FirstOrDefault(x => x.Type == type && x.SkillScores != null && x.SkillScores.Any());
                if (videoSkillScore != null && videoSkillScore.SkillScores != null && videoSkillScore.SkillScores.Any())
                {
                    var skillScores = videoSkillScore.SkillScores.GroupBy(x => x.Skill)
                       .Select(x => GetSkillScore(x))
                       .ToList();
                    var percent = GetDoublePercent(skillScores, percentSkill, unitIds.Count);
                    skillScorePercents.Add((skillScores, percent));
                }
            }
            if (skillScorePercents.Any())
            {
                var skillScoreSkills = skillScorePercents.SelectMany(x => x.Item1).GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList();
                return (skillScoreSkills, NumberHelper.ConvertRound(skillScorePercents.Sum(x => x.Item2)));
            }

            return (new List<SkillScores>(), default);
        }

        private async Task<(List<SkillScores>, double)> GetFinalTestSkillScore(Guid? finalTestId, Course course, Guid studentId)
        {
            if (!finalTestId.HasValue)
            {
                return (new List<SkillScores>(), default);
            }
            var finalTestResult = await _finalTestResultRepository.Queryable.Where(x => x.CourseId == course.Id && x.FinalTestId == finalTestId)
                                                                            .FirstOrDefaultAsync(x => x.StudentId == studentId && x.Status == EnumResultStatus.Done);
            var skillScores = finalTestResult?.SkillScores?.GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList() ?? new List<SkillScores>();
            var percentSkill = 0;
            if (course.CourseType == EnumCourseType.Academic)
            {
                percentSkill = OverallPercentCourse.OverallAcaPercentFinalTest;
            }
            else if (course.CourseType == EnumCourseType.EnglishFoundation)
            {
                percentSkill = OverallPercentCourse.OverallRFIPercentFinalTest;
            }
            return (skillScores, NumberHelper.ConvertDoublePercent((finalTestResult?.Percent ?? default) * percentSkill));
        }

        public async Task UpdateProcessUnit(UnitResult? unitResult, CancellationToken cancellationToken)
        {
            if (unitResult == null)
            {
                return;
            }
            var course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests).FirstOrDefaultAsync(x => x.Id == unitResult.CourseId, cancellationToken);
            if (course == null)
            {
                return;
            }
            var courseUnitMockTests = course.CourseUnitMockTests.OrderBy(x => x.DisplayOrder).ThenBy(x => x.CreatedDate).ToList();
            var courseUnitMockTest = GetCourseUnitMockTest(courseUnitMockTests, unitResult.UnitId, nameof(unitResult.UnitId));
            if (courseUnitMockTest != null)
            {
                await UpdateStatusProcess(courseUnitMockTest, unitResult.StudentId, cancellationToken);
            }
        }

        private static CourseUnitMockTest? GetCourseUnitMockTest(IList<CourseUnitMockTest>? courseUnitMockTests, Guid objectId, string? type)
        {
            if (courseUnitMockTests != null && courseUnitMockTests.Any())
            {
                var courseUnitMockTest = courseUnitMockTests.Where(x => x.GetPropValue<Guid>(type) == objectId).FirstOrDefault();
                if (courseUnitMockTest == null)
                {
                    return default;
                }
                var index = courseUnitMockTests.IndexOf(courseUnitMockTest) + 1;
                if (index < courseUnitMockTests.Count)
                {
                    return courseUnitMockTests[index];
                }
            }
            return default;
        }

        public async Task UpdateProcessMockTest(MockTestResult? mockTestResult, CancellationToken cancellationToken)
        {
            if (mockTestResult == null)
            {
                return;
            }

            var course = await GetCourseAsync(mockTestResult.StudentId, mockTestResult.CourseId, cancellationToken);
            if (course == null)
            {
                return;
            }
            var courseUnitMockTests = course.CourseUnitMockTests.OrderBy(x => x.DisplayOrder).ThenBy(x => x.CreatedDate).ToList();
            var courseUnitMockTest = GetCourseUnitMockTest(courseUnitMockTests, mockTestResult.MockTestId, nameof(mockTestResult.MockTestId));
            var isCheckDone = await CheckCourseIELTSDoneAsync(mockTestResult.StudentId, mockTestResult.CourseId, cancellationToken);
            if (!isCheckDone && courseUnitMockTest != null)
            {
                await UpdateStatusProcess(courseUnitMockTest, mockTestResult.StudentId, cancellationToken);
            }
            else if (isCheckDone)
            {
                await UpdateCourseResult(course, mockTestResult.StudentId, cancellationToken);
            }
        }

        public async Task<Course?> GetCourseAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken)
        {
            return await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests).Include(x => x.CourseResults.Where(x => x.CourseId == courseId && x.StudentId == studentId))
                                .FirstOrDefaultAsync(x => x.Id == courseId, cancellationToken);
        }

        public async Task<bool> CheckCourseIELTSDoneAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken)
        {
            var isUnitDone = await _unitResultRepository.Queryable.Where(x => x.StudentId == studentId && x.CourseId == courseId)
                .Select(x => x.Status).AllAsync(x => x == EnumResultStatus.Done, cancellationToken);
            var isMockTestDone = await _mockTestResultRepository.Queryable.Where(x => x.StudentId == studentId && x.CourseId == courseId)
                 .Select(x => x.Status).AllAsync(x => x == EnumResultStatus.Done, cancellationToken);
            return isUnitDone && isMockTestDone;
        }

        private async Task UpdateStatusProcess(CourseUnitMockTest? courseUnitMockTest, Guid studentId, CancellationToken cancellationToken)
        {
            if (courseUnitMockTest == null)
            {
                return;
            }
            switch (true)
            {
                case var value when value == courseUnitMockTest.UnitId.HasValue:
                    var unitResultNext = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.UnitId == courseUnitMockTest.UnitId && x.CourseId == courseUnitMockTest.CourseId, cancellationToken);
                    if (unitResultNext == null)
                    {
                        break;
                    }
                    var currentStatusResult = await _orderService.GetCurrentStatusAsync(unitResultNext.CreatedUserId);
                    var currentAccountStatus = currentStatusResult?.Content?.Result ?? default;
                    await UpdateStudentTrialRegistration(currentAccountStatus, unitResultNext.CreatedUserId);
                    if (unitResultNext.Status == EnumResultStatus.Unfinished && currentAccountStatus == EnumTrialRegistrationStatus.Payment)
                    {
                        unitResultNext.NewDate = DateTime.UtcNow;
                        unitResultNext.Status = EnumResultStatus.New;
                        await _unitResultRepository.BulkUpdateList(new List<UnitResult> { unitResultNext }, bulk =>
                        {
                            bulk.IgnoreOnUpdateExpression = c => new { c.UnitId, c.StudentId, c.CourseId };
                        });
                    }
                    break;

                case var value when value == courseUnitMockTest.FinalTestId.HasValue:
                    var finalTestResultNext = await _finalTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.FinalTestId == courseUnitMockTest.FinalTestId && x.CourseId == courseUnitMockTest.CourseId, cancellationToken);
                    if (finalTestResultNext != null && finalTestResultNext.Status == EnumResultStatus.Unfinished)
                    {
                        finalTestResultNext.NewDate = DateTime.UtcNow;
                        finalTestResultNext.Status = EnumResultStatus.New;
                        await _finalTestResultRepository.BulkUpdateList(new List<FinalTestResult> { finalTestResultNext }, bulk =>
                        {
                            bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.FinalTestId };
                        });
                    }
                    break;

                case var value when value == courseUnitMockTest.MockTestId.HasValue:
                    var mockTestResultNext = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.MockTestId == courseUnitMockTest.MockTestId && x.CourseId == courseUnitMockTest.CourseId, cancellationToken);
                    if (mockTestResultNext != null && mockTestResultNext.Status == EnumResultStatus.Unfinished)
                    {
                        mockTestResultNext.NewDate = DateTime.UtcNow;
                        mockTestResultNext.Status = EnumResultStatus.New;
                        await _mockTestResultRepository.BulkUpdateList(new List<MockTestResult> { mockTestResultNext }, bulk =>
                        {
                            bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.UnitId, c.MockTestId };
                        });
                    }
                    break;

                default:
                    break;
            }
        }

        public async Task<CourseResult?> UpdateCourse(Course? course, Guid studentId, CancellationToken cancellationToken)
        {
            var courseResult = course?.CourseResults.FirstOrDefault(x => x.StudentId == studentId);
            if (course == null)
            {
                return courseResult;
            }
            try
            {
                var finalTestId = course.CourseUnitMockTests.Where(x => x.FinalTestId.HasValue).FirstOrDefault()?.FinalTestId;
                var unitIds = course.CourseUnitMockTests.Where(x => x.UnitId.HasValue).Select(x => x.UnitId.GetValueOrDefault()).ToList();
                var (skillScores, percent) = await GetCourseResult(course, unitIds, studentId, finalTestId);
                if (courseResult != null)
                {
                    courseResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                    courseResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
                    courseResult.Percent = NumberHelper.ConvertRound(percent);
                    courseResult.SkillScores = skillScores;
                    await _courseResultRepository.BulkUpdateList(new List<CourseResult> { courseResult }, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId };
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Trigger Update CourseResult : {ex.Message} ");
            }

            return courseResult;
        }

        public async Task UpdateCourseResult(Course? course, Guid studentId, CancellationToken cancellationToken)
        {
            if (course == null)
            {
                return;
            }

            var courseResult = await UpdateCourse(course, studentId, cancellationToken);
            if (courseResult == null)
            {
                return;
            }

            //var userId = courseResult.CreatedUserId;
            //await DoQuestBoard(courseId, userId, cancellationToken);
            try
            {
                if (courseResult.Status != EnumResultStatus.Done)
                {
                    await _sendMailFinishCoursePublisher.Publish(new SendMailFinishCourseModel() { StudentId = studentId, CourseId = courseResult.CourseId }, cancellationToken);

                    await SendNotificationMessage(courseResult.CreatedUserId, course.Name, cancellationToken);
                    await _userService.UpdateStudentByLevelAsync(new UpdateStudentByLevelModel
                    {
                        BaseCourseLevel = course.CourseLevel,
                        CourseLevel = course.CourseLevel,
                        Id = courseResult.CreatedUserId,
                    }).ConfigureAwait(false);

                    await _saveUserCourseSettingPublisher.Publish(new SaveUserCourseSettingQueueModel
                    {
                        CourseLevel = course.CourseLevel,
                        Type = EnumUserCourseType.ResetAndLearnAgain,
                        UserId = courseResult.CreatedUserId
                    }, cancellationToken).ConfigureAwait(false);

                    await _saveUserCourseSettingPublisher.Publish(new SaveUserCourseSettingQueueModel
                    {
                        Type = EnumUserCourseType.ChangeLevel,
                        UserId = courseResult.CreatedUserId
                    }, cancellationToken).ConfigureAwait(false);

                    courseResult.CompletionDate = DateTime.UtcNow;
                }

                courseResult.Status = EnumResultStatus.Done;
                await _courseResultRepository.BulkUpdateList(new List<CourseResult> { courseResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId };
                });

                await _sendMailFinishCoursePublisher.Publish(new SendMailFinishCourseModel() { StudentId = studentId, CourseId = courseResult.CourseId }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Trigger Update Status CourseResult : {ex.Message} ");
            }
        }

        private async Task UpdateStudentTrialRegistration(EnumTrialRegistrationStatus status, Guid userId)
        {
            if (status == EnumTrialRegistrationStatus.Trial)
            {
                StudentTrialRegistrationModel model = new StudentTrialRegistrationModel()
                {
                    UserId = userId,
                    Status = EnumTrialRegistrationStatus.Finished
                };
                await _userService.UpdateTrialRegistrationStatusAsync(model);
            }
        }

        private async Task SendNotificationMessage(Guid userId, string? courseName, CancellationToken cancellationToken)
        {
            NotificationSendingQueueModel model = new NotificationSendingQueueModel()
            {
                ObjectId = Guid.Empty,
                UserIds = new List<Guid>() { userId },
                SenderId = Guid.Empty,
                ParamsMessage = new List<object> { courseName ?? string.Empty, },
                Type = EnumNotificationType.LinkPage,
                Content = EnumNotificationContent.CompleteCourse
            };

            await _notificationMessagePublisher.Publish(model, cancellationToken).ConfigureAwait(false);
        }
    }
}
