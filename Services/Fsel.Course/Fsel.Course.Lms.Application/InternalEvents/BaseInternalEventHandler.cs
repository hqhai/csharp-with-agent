// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.SenderTemplates;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

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
        protected readonly SaveUserCourseSettingPublisher _saveUserCourseSettingPublisher;
        protected readonly IMediator _mediator;
        protected readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        protected readonly AppSetting _appSetting;
        protected readonly ISystemService _systemService;
        protected readonly IOrderService _orderService;
        protected readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private const int PercentClassForumAcademic = 20;
        private const int PercentClassForumIELST = 32;
        private const int PercentHomeWorkAcademic = 14;
        private const int PercentHomeWorkIELST = 48;
        private const int PercentFinalTest = 15;
        private const int PercentVideoAcademic = 9;
        private const double PercentVideoIELSTWS = 3.5;
        private const double PercentVideoIELST = 3.25;
        private const int PercentUnitTest = 24;
        private const int PercentSkillTest = 18;

        public BaseInternalEventHandler(ISystemService systemService, AppSetting appSetting,
            ICourseUnitMockTestRepository courseUnitMockTestRepository,
            IMediator mediator,
            IUserService userService,
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
            IOrderService orderService
, NotificationMessagePublisher notificationMessagePublisher)
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
            _saveUserCourseSettingPublisher = saveUserCourseSettingPublisher;
            _mediator = mediator;
            _appSetting = appSetting;
            _systemService = systemService;
            _questBoardPublisher = questBoardPublisher;
            _orderService = orderService;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        private async Task<(List<SkillScores>, double)> GetCourseResult(Course course, IList<Guid> unitIds, Guid studentId, Guid? finalTestId)
        {
            ArgumentNullException.ThrowIfNull(unitIds);
            Thread.Sleep(2000);
            var units = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.StudentId == studentId && x.CourseId == course.Id)).Where(x => unitIds.Contains(x.Id)).ToListAsync();
            var lessonResults = units.SelectMany(x => x.LessonResults).Where(x => x.StudentId == studentId && x.CourseId == course.Id).ToList();

            var (videoSkillScores, percentVideo) = await GetVideoSkillScores(lessonResults.Select(x => x.Id).ToList(), EnumTimeCodeType.Standalone, default, course.CourseType);
            var (homeWorkSkillScores, percentHomeWork) = await GetHomeWordsSkillScores(lessonResults.Select(x => x.Id).ToList(), default, course.CourseType);
            var (classForumSkillScores, percentClassForum) = await GetClassForumSkillScores(lessonResults.Select(x => x.Id).ToList(), default, course.CourseType);
            var percents = new List<double> { percentClassForum, percentHomeWork, percentVideo };
            List<SkillScores> mergedSkillScores = videoSkillScores.Concat(homeWorkSkillScores).Concat(classForumSkillScores).ToList();
            if (course.CourseType == EnumCourseType.Academic)
            {
                var (unitSkillScores, percentUnitSkill) = await GetSkillScoreByCourses(course.Id, unitIds, studentId, EnumTimeCodeType.UnitTest, PercentUnitTest);
                if (!unitSkillScores.Any())
                {
                    percentUnitSkill = PercentUnitTest;
                }
                var (skillSkillScores, percentSkill) = await GetSkillScoreByCourses(course.Id, unitIds, studentId, EnumTimeCodeType.SkillTest, PercentSkillTest);
                if (!skillSkillScores.Any())
                {
                    percentSkill = PercentSkillTest;
                }
                var (finalTestSkillScores, percentFinalTest) = await GetFinalTestSkillScore(finalTestId, course.Id, studentId);
                mergedSkillScores = mergedSkillScores.Concat(skillSkillScores).Concat(unitSkillScores).Concat(finalTestSkillScores).ToList();
                percents.AddRange(new List<double> { percentUnitSkill, percentSkill, percentFinalTest });
            }
            List<SkillScores> groupedSkillScores = mergedSkillScores.GroupBy(x => x.Skill).Select(group => GetSumSkillScore(group)).OrderBy(x => x.Skill).ToList();
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
                if (courseType == EnumCourseType.Academic)
                {
                    return (skillScores, GetDoublePercent(skillScores, PercentVideoAcademic));
                }
                else if (courseType != null)
                {
                    return (skillScores, skillScores.Any() ? NumberHelper.ConvertDoublePercent(skillScores.Sum(x =>
                    {
                        if (x.Skill == EnumCourseSkill.Writing || x.Skill == EnumCourseSkill.Speaking)
                        {
                            return NumberHelper.ConvertRound(x.Percent * PercentVideoIELSTWS);
                        }
                        return NumberHelper.ConvertRound(x.Percent * PercentVideoIELST);
                    })) : default);
                }
            }
            return (skillScores, GetDoublePercentUnit(skillScores, percentSkill));
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
                if (courseType != null && courseType == EnumCourseType.Academic)
                {
                    return (skillScores, GetDoublePercent(skillScores, PercentClassForumAcademic));
                }
                else if (courseType != null && courseType == EnumCourseType.Ielts)
                {
                    return (skillScores, GetDoublePercent(skillScores, PercentClassForumIELST));
                }
            }
            return (skillScores, GetDoublePercentUnit(skillScores, percentSkill));
        }

        public async Task<(List<SkillScores>, double)> GetHomeWordsSkillScores(IList<Guid>? lessonResultIds, int percentSkill = default, EnumCourseType? courseType = null)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            List<SkillScores> skillScores = new List<SkillScores>();
            var homeWorkResults = await _homeWorkResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
            if (homeWorkResults != null)
            {
                skillScores = homeWorkResults.Where(x => x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!).GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList();
                if (courseType != null && courseType == EnumCourseType.Academic && skillScores.Any())
                {
                    return (skillScores, GetDoublePercent(skillScores, PercentHomeWorkAcademic));
                }
                else if (courseType != null && courseType == EnumCourseType.Ielts && skillScores.Any())
                {
                    return (skillScores, GetDoublePercent(skillScores, PercentHomeWorkIELST));
                }
                else
                {
                    return (skillScores, GetDoublePercentUnit(skillScores, percentSkill));
                }
            }
            return (skillScores, default);
        }

        private static double GetDoublePercent(IList<SkillScores>? skillScores, int percentOccupy, int numberOfElements = default)
        {
            if (skillScores != null && skillScores.Any())
            {
                if (numberOfElements != default)
                {
                    return NumberHelper.ConvertDoublePercent(skillScores.Average(x => x.Percent * percentOccupy / numberOfElements));
                }
                else
                {
                    return NumberHelper.ConvertDoublePercent(skillScores.Sum(x => x.Percent * percentOccupy / skillScores.Count));
                }
            }

            return default;
        }

        private static double GetDoublePercentUnit(IList<SkillScores>? skillScores, int percentOccupy)
        {
            if (skillScores != null && skillScores.Any())
            {
                return NumberHelper.ConvertDoublePercent(skillScores.Sum(x => x.Percent * percentOccupy / skillScores.Count));
            }
            return default;
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
            };
            return skillScores;
        }

        private async Task<(List<SkillScores>, double)> GetSkillScoreByCourses(Guid courseId, IList<Guid>? unitIds, Guid studentId, EnumTimeCodeType type, int percentSkill = default)
        {
            ArgumentNullException.ThrowIfNull(unitIds);
            var skillScorePercents = new List<(List<SkillScores>, double)>();

            foreach (var unitId in unitIds)
            {
                var unit = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.StudentId == studentId && x.CourseId == courseId)).FirstOrDefaultAsync(x => x.Id == unitId);
                var lessonResultIds = unit?.LessonResults.Where(x => x.StudentId == studentId && x.CourseId == courseId && x.Status == EnumResultStatus.Done).Select(x => x.Id).ToList();
                if (lessonResultIds == null || !lessonResultIds.Any())
                {
                    break;
                }
                var videoResults = await _videoResultRepository.Queryable.Where(x => lessonResultIds.Contains(x.LessonResultId) && x.Status == EnumResultStatus.Done).ToListAsync();
                var videoSkillScore = videoResults.Where(x => x.VideoSkillScores != null && x.VideoSkillScores.Any()).SelectMany(x => x.VideoSkillScores!).FirstOrDefault(x => x.Type == type && x.SkillScores != null && x.SkillScores.Any());
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

        private async Task<(List<SkillScores>, double)> GetFinalTestSkillScore(Guid? finalTestId, Guid courseId, Guid studentId)
        {
            if (finalTestId == null)
            {
                return (new List<SkillScores>(), default);
            }

            var finalTestResult = await _finalTestResultRepository.Queryable.Where(x => x.CourseId == courseId && x.FinalTestId == finalTestId)
                                                                            .FirstOrDefaultAsync(x => x.StudentId == studentId && x.Status == EnumResultStatus.Done);
            var skillScores = finalTestResult?.SkillScores?.GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList();
            var percent = NumberHelper.ConvertDoublePercent(finalTestResult?.Percent * PercentFinalTest ?? default);
            return (skillScores ?? new List<SkillScores>(), percent);
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
            switch (false)
            {
                case var value when value == (courseUnitMockTest.UnitId == null):
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
                        unitResultNext.Status = EnumResultStatus.New;
                        _unitResultRepository.Update(unitResultNext);
                        await _unitResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    break;

                case var value when value == (courseUnitMockTest.FinalTestId == null):
                    var finalTestResultNext = await _finalTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.FinalTestId == courseUnitMockTest.FinalTestId && x.CourseId == courseUnitMockTest.CourseId, cancellationToken);
                    if (finalTestResultNext != null && finalTestResultNext.Status == EnumResultStatus.Unfinished)
                    {
                        finalTestResultNext.Status = EnumResultStatus.New;
                        _finalTestResultRepository.Update(finalTestResultNext);
                        await _finalTestResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    break;

                case var value when value == (courseUnitMockTest.MockTestId == null):
                    var mockTestResultNext = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.MockTestId == courseUnitMockTest.MockTestId && x.CourseId == courseUnitMockTest.CourseId, cancellationToken);
                    if (mockTestResultNext != null && mockTestResultNext.Status == EnumResultStatus.Unfinished)
                    {
                        mockTestResultNext.Status = EnumResultStatus.New;
                        _mockTestResultRepository.Update(mockTestResultNext);
                        await _mockTestResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    break;

                default:
                    break;
            }
        }

        public async Task<CourseResult?> UpdateCourse(Course? course, Guid studentId, CancellationToken cancellationToken)
        {
            var courseResult = course?.CourseResults.FirstOrDefault(x => x.StudentId == studentId);
            if (course != null)
            {
                var finalTestId = course.CourseUnitMockTests.Where(x => x.FinalTestId != null).FirstOrDefault()?.FinalTestId;
                var unitIds = course.CourseUnitMockTests.Where(x => x.UnitId != null).Select(x => x.UnitId ?? default).ToList();
                var courseType = course.CourseLevel.GetEnumCourseType();
                var (skillScores, percent) = await GetCourseResult(course, unitIds, studentId, finalTestId);
                if (courseResult != null)
                {
                    courseResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                    courseResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
                    courseResult.Percent = NumberHelper.ConvertRound(percent);
                    courseResult.SkillScores = skillScores;
                    _courseResultRepository.Update(courseResult);
                    await _courseResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
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

            if (courseResult.Status != EnumResultStatus.Done)
            {
                //await SendStudentCompleteCourse(studentId, course.Id, courseResult, cancellationToken);
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
            _courseResultRepository.Update(courseResult);
            await _courseResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task SendStudentCompleteCourse(Guid studentId, Guid courseId, CourseResult courseResult, CancellationToken cancellationToken)
        {
            var studentResult = await _userService.GetUserByStudentId(courseResult.StudentId);
            if (!studentResult.IsSuccessStatusCode)
            {
                return;
            }
            var student = studentResult.Content?.Result;
            var course = await _courseRepository.GetByIdAsync(courseId);
            var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel { CourseId = courseId, UserId = courseResult.CreatedUserId });
            var featureAccessTime = featureAccessTimeResult.Content?.Result;
            var sendStudentCompleteCourseModel = new SendStudentCompleteCourseModel
            {
                StudentName = student?.Human?.FullName,
                CourseName = course?.Name,
                NumberOfHour = featureAccessTime == null ? "0" : Math.Round(((double)featureAccessTime.AccessTime / 3600), 2).ToString(CultureInfo.CurrentCulture),
                NumberOfUnit = _courseUnitMockTestRepository.Queryable.Where(p => p.CourseId == courseId && p.UnitId.HasValue).Count().ToString(CultureInfo.CurrentCulture),
                LevelOfStudent = student?.CourseLevel.ToString(),
                AccessLink = _appSetting.ResourceContent?.LmsWebsiteUrl,
                HotLine = _appSetting.ResourceContent?.HotLine
            };
            if (course?.CourseType == EnumCourseType.Academic)
            {
                var finalTestResult = await _finalTestResultRepository.Queryable.FirstOrDefaultAsync(p => p.CourseId == courseId && p.StudentId == studentId, cancellationToken);
                sendStudentCompleteCourseModel.GrammarScore = finalTestResult?.SkillScores?.FirstOrDefault(p => p.Skill == EnumCourseSkill.Grammar)?.Percent.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
                sendStudentCompleteCourseModel.ReadingScore = finalTestResult?.SkillScores?.FirstOrDefault(p => p.Skill == EnumCourseSkill.Reading)?.Percent.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
                sendStudentCompleteCourseModel.VocabularyScore = finalTestResult?.SkillScores?.FirstOrDefault(p => p.Skill == EnumCourseSkill.Vocabulary)?.Percent.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
            }
            else
            {
                var mockTestResult = await _mockTestResultRepository.Queryable.Where(p => p.CourseId == courseId && p.StudentId == studentId).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);
                sendStudentCompleteCourseModel.SpeakingScore = mockTestResult?.SkillScores?.FirstOrDefault(p => p.Skill == EnumCourseSkill.Speaking)?.Percent.ToString(CultureInfo.CurrentCulture);
                sendStudentCompleteCourseModel.ReadingScore = mockTestResult?.SkillScores?.FirstOrDefault(p => p.Skill == EnumCourseSkill.Reading)?.Percent.ToString(CultureInfo.CurrentCulture);
                sendStudentCompleteCourseModel.WritingScore = mockTestResult?.SkillScores?.FirstOrDefault(p => p.Skill == EnumCourseSkill.Writing)?.Percent.ToString(CultureInfo.CurrentCulture);
                sendStudentCompleteCourseModel.ListeningScore = mockTestResult?.SkillScores?.FirstOrDefault(p => p.Skill == EnumCourseSkill.Listening)?.Percent.ToString(CultureInfo.CurrentCulture);
            }
            var sendResult = await _mediator.Send(new SenderCommand
            {
                Email = student?.Human?.Email,
                Subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendStudentCompleteCourse, course?.Name, student?.Human?.FullName),
                Params = sendStudentCompleteCourseModel,
                Template = course?.CourseType == EnumCourseType.Academic ? EnumSenderTemplate.SendStudentCompleteCourseAcademic : EnumSenderTemplate.SendStudentCompleteCourseIetls
            }, cancellationToken).ConfigureAwait(false);
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

        private async Task SendNotificationMessage(Guid userId, string courseName, CancellationToken cancellationToken)
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
