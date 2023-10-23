// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.SenderTemplates;
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
        protected readonly IMediator _mediator;
        protected readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        protected readonly FinishOneLevelPassPublisher _finishOneLevelPassPublisher;
        protected readonly AppSetting _appSetting;
        protected readonly ISystemService _systemService;
        private const int TotalScoreClassForum = 36;
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
            FinishOneLevelPassPublisher finishOneLevelPassPublisher,
            ICourseUnitMockTestRepository courseUnitMockTestRepository,
            IMediator mediator,
            IUserService userService,
            IVideoResultRepository videoResultRepository,
            IClassForumResultRepository classForumResultRepository,
            IUnitResultRepository unitResultRepository,
            ICourseResultRepository courseResultRepository,
            ICourseRepository courseRepository,
            IUnitRepository unitRepository,
            IFinalTestResultRepository finalTestResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository)
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
            _finishOneLevelPassPublisher = finishOneLevelPassPublisher;
            _userService = userService;
            _mediator = mediator;
            _appSetting = appSetting;
            _systemService = systemService;
        }

        private async Task<(List<SkillScores>, double)> GetCourseResult(IList<Guid> unitIds, Guid studentId, EnumCourseType type, Guid? finalTestId)
        {
            ArgumentNullException.ThrowIfNull(unitIds);
            var units = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.StudentId == studentId)).Where(x => unitIds.Contains(x.Id)).ToListAsync();
            var lessonResults = units.SelectMany(x => x.LessonResults).Where(x => x.StudentId == studentId).ToList();
            var (videoSkillScores, percentVideo) = await GetVideoSkillScores(lessonResults.Select(x => x.Id).ToList(), EnumTimeCodeType.Standalone, default, type);
            var (homeWorkSkillScores, percentHomeWork) = await GetHomeWordsSkillScores(lessonResults.Select(x => x.Id).ToList(), default, type);
            var (classForumSkillScores, percentClassForum) = await GetClassForumSkillScores(lessonResults.Select(x => x.Id).ToList(), default, type);
            var percents = new List<double> { percentClassForum, percentHomeWork, percentVideo };
            List<SkillScores> mergedSkillScores = videoSkillScores.Concat(homeWorkSkillScores).Concat(classForumSkillScores).ToList();
            if (type == EnumCourseType.Academic)
            {
                var (unitSkillScores, percentUnitSkill) = await GetSkillScoreByCourses(unitIds, studentId, EnumTimeCodeType.UnitTest, PercentUnitTest);
                if (!unitSkillScores.Any())
                {
                    percentUnitSkill = PercentUnitTest;
                }
                var (skillSkillScores, percentSkill) = await GetSkillScoreByCourses(unitIds, studentId, EnumTimeCodeType.SkillTest, PercentSkillTest);
                if (!skillSkillScores.Any())
                {
                    percentSkill = PercentSkillTest;
                }
                var (finalTestSkillScores, percentFinalTest) = await GetFinalTestSkillScore(finalTestId, studentId);
                mergedSkillScores = mergedSkillScores.Concat(skillSkillScores).Concat(unitSkillScores).Concat(finalTestSkillScores).ToList();
                percents.AddRange(new List<double> { percentUnitSkill, percentSkill, percentFinalTest });
            }
            List<SkillScores> groupedSkillScores = mergedSkillScores.GroupBy(x => x.Skill).Select(group => GetSumSkillScore(group)).ToList();
            return (groupedSkillScores, percents.Sum());
        }

        public async Task<(List<SkillScores>, double)> GetVideoSkillScores(IList<Guid>? lessonResultIds, EnumTimeCodeType type, int percentSkill = default, EnumCourseType? courseType = null)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            List<SkillScores> skillScores = new List<SkillScores>();
            var videoResults = await _videoResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
            if (videoResults != null && videoResults.Any())
            {
                skillScores = videoResults.SelectMany(x => x.VideoSkillScores!).Where(x => x.Type == type && x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!).GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList();
                if (courseType == EnumCourseType.Academic)
                {
                    return (skillScores, GetDoublePercent(skillScores, PercentVideoAcademic));
                }
                else if (courseType != null)
                {
                    return (skillScores, skillScores.Any() ? NumberHelper.ConvertDoublePercent(skillScores.Average(x =>
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
            var classForumResults = await _classForumResultRepository.Queryable.Include(x => x.ClassForum).Include(x => x.ClassForumScores).Where(x => x.Status == EnumClassForumResultStatus.Graded && lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
            if (classForumResults != null && classForumResults.Any())
            {
                skillScores = classForumResults.Select(x => GetSkillScores(x)).ToList().GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList();
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

        public SkillScores GetSkillScores(ClassForumResult classForumResult)
        {
            ArgumentNullException.ThrowIfNull(classForumResult);
            return new SkillScores
            {
                Skill = classForumResult.ClassForum?.CourseSkill ?? default,
                TotalQuestion = 1,
                CountQuestion = 1,
                TotalCount = TotalScoreClassForum,
                CorrectCount = classForumResult.ClassForumScores.Sum(x => x.Score),
                Percent = NumberHelper.ConvertPercentDouble((double)classForumResult.ClassForumScores.Sum(x => x.Score) / TotalScoreClassForum)
            };
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
                    Percent = NumberHelper.ConvertRound(group.Average(x => x.Percent), 2),
                };
            }
            return new SkillScores();
        }

        private static SkillScores GetSkillScore(IGrouping<EnumCourseSkill, SkillScores>? x)
        {
            SkillScores skillScores = new SkillScores();
            if (x != null)
            {
                skillScores.Skill = x.Key;
                skillScores.Scores = x.Sum(x => x.Scores) > 0 ? x.Average(x => x.Scores) : default;
                skillScores.TotalQuestion = x.Sum(x => x.TotalQuestion);
                skillScores.CountQuestion = x.Sum(x => x.CountQuestion);
                skillScores.TotalCount = x.Sum(x => x.TotalCount);
                skillScores.CorrectCount = x.Sum(x => x.CorrectCount);
                skillScores.Percent = x.Sum(x => x.TotalCount) > 0 ? NumberHelper.ConvertPercentDouble(x.Sum(x => x.CorrectCount) / x.Sum(x => x.TotalCount)) : default;
                return skillScores;
            };
            return skillScores;
        }

        private async Task<(List<SkillScores>, double)> GetSkillScoreByCourses(IList<Guid>? unitIds, Guid studentId, EnumTimeCodeType type, int percentSkill = default)
        {
            ArgumentNullException.ThrowIfNull(unitIds);
            var skillScorePercents = new List<(List<SkillScores>, double)>();

            foreach (var unitId in unitIds)
            {
                var unit = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.StudentId == studentId)).FirstOrDefaultAsync(x => x.Id == unitId);
                var lessonResultIds = unit?.LessonResults.Where(x => x.StudentId == studentId && x.Status == EnumResultStatus.Done).Select(x => x.Id).ToList();
                if (lessonResultIds != null && lessonResultIds.Any())
                {
                    var videoResults = await _videoResultRepository.Queryable.Where(x => lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
                    var videoSkillScore = videoResults.SelectMany(x => x.VideoSkillScores!).FirstOrDefault(x => x.Type == type && x.SkillScores != null && x.SkillScores.Any());
                    if (videoSkillScore != null && videoSkillScore.SkillScores != null && videoSkillScore.SkillScores.Any())
                    {
                        var skillScores = videoSkillScore.SkillScores.GroupBy(x => x.Skill)
                           .Select(x => GetSkillScore(x))
                           .ToList();
                        var percent = GetDoublePercent(skillScores, percentSkill, unitIds.Count);
                        skillScorePercents.Add((skillScores, percent));
                    }
                }
            }
            if (skillScorePercents.Any())
            {
                var skillScoreSkills = skillScorePercents.SelectMany(x => x.Item1).GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList();
                return (skillScoreSkills, NumberHelper.ConvertRound(skillScorePercents.Average(x => x.Item2), 2));
            }

            return (new List<SkillScores>(), default);
        }

        private async Task<(List<SkillScores>, double)> GetFinalTestSkillScore(Guid? finalTestId, Guid studentId)
        {
            if (finalTestId == null)
            {
                return (new List<SkillScores>(), default);
            }

            var finalTestResult = await _finalTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.FinalTestId == finalTestId && x.StudentId == studentId && x.Status == EnumResultStatus.Done);
            var skillScores = finalTestResult?.SkillScores?.GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList();
            var percent = NumberHelper.ConvertDoublePercent(finalTestResult?.Percent * PercentFinalTest ?? default);
            return (skillScores ?? new List<SkillScores>(), percent);
        }

        public async Task UpdateProcessUnit(UnitResult? unitResult, CancellationToken cancellationToken)
        {
            var courseId = unitResult != null ? unitResult.CourseId : default;
            if (courseId != default && unitResult != null)
            {
                var course = await _courseRepository.GetIncludeCourseUnitMockTestByIdAsync(courseId);
                if (course != null)
                {
                    var courseUnitMockTests = course.CourseUnitMockTests.OrderBy(x => x.DisplayOrder).ToList();
                    var index = courseUnitMockTests.FindIndex(x => x.UnitId == unitResult.UnitId) + 1;
                    if (index < courseUnitMockTests.Count)
                    {
                        await UpdateStatusProcess(courseUnitMockTests[index], unitResult.StudentId, cancellationToken);
                    }
                }
            }
        }

        public async Task UpdateProcessMockTest(MockTestResult? mockTestResult, CancellationToken cancellationToken)
        {
            var courseId = mockTestResult != null ? mockTestResult.CourseId : default;
            if (courseId != default && mockTestResult != null)
            {
                var course = await _courseRepository.GetIncludeCourseUnitMockTestByIdAsync(courseId);
                if (course != null)
                {
                    var isCheckUnitResults = course.UnitResults.Where(x => x.StudentId == mockTestResult.StudentId).All(x => x.Status == EnumResultStatus.Done);
                    var isCheckDone = isCheckUnitResults && course.MockTestResults.Where(x => x.StudentId == mockTestResult.StudentId).All(x => x.Status == EnumResultStatus.Done);
                    var displayOrder = course.CourseUnitMockTests.FirstOrDefault(x => x.MockTestId == mockTestResult.MockTestId)?.DisplayOrder;
                    var courseUnitMockTest = course.CourseUnitMockTests.FirstOrDefault(x => x.DisplayOrder == displayOrder + 1);
                    if (!isCheckDone && courseUnitMockTest != null)
                    {
                        await UpdateStatusProcess(courseUnitMockTest, mockTestResult.StudentId, cancellationToken);
                    }
                    else
                    {
                        await UpdateCourseResult(courseId, mockTestResult.StudentId, cancellationToken);
                    }
                }
            }
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
                    var unitResultNext = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.UnitId == courseUnitMockTest.UnitId, cancellationToken);
                    if (unitResultNext != null)
                    {
                        unitResultNext.Status = EnumResultStatus.New;
                        _unitResultRepository.Update(unitResultNext);
                        await _unitResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    break;

                case var value when value == (courseUnitMockTest.FinalTestId == null):
                    var finalTestResultNext = await _finalTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.FinalTestId == courseUnitMockTest.FinalTestId, cancellationToken);
                    if (finalTestResultNext != null)
                    {
                        finalTestResultNext.Status = EnumResultStatus.New;
                        _finalTestResultRepository.Update(finalTestResultNext);
                        await _finalTestResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    break;

                case var value when value == (courseUnitMockTest.MockTestId == null):
                    var mockTestResultNext = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.MockTestId == courseUnitMockTest.MockTestId, cancellationToken);
                    if (mockTestResultNext != null)
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

        public async Task UpdateCourse(Guid courseId, Guid studentId, CancellationToken cancellationToken)
        {
            var course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests).FirstOrDefaultAsync(x => x.Id == courseId, cancellationToken);
            if (course != null)
            {
                var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == courseId && x.StudentId == studentId, cancellationToken);
                var finalTestId = course.CourseUnitMockTests.Where(x => x.FinalTestId != null).FirstOrDefault()?.FinalTestId;
                var unitIds = course.CourseUnitMockTests.Where(x => x.UnitId != null).Select(x => x.UnitId ?? default).ToList();
                var courseType = course.CourseLevel.GetEnumCourseType();
                var (skillScores, percent) = await GetCourseResult(unitIds, studentId, courseType, finalTestId).ConfigureAwait(false);
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
        }

        public async Task UpdateCourseResult(Guid courseId, Guid studentId, CancellationToken cancellationToken)
        {
            await UpdateCourse(courseId, studentId, cancellationToken).ConfigureAwait(false);
            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == courseId && x.StudentId == studentId, cancellationToken);
            if (courseResult != null)
            {
                await _finishOneLevelPassPublisher.Publish(courseResult, cancellationToken);
                courseResult.Status = EnumResultStatus.Done;
                _courseResultRepository.Update(courseResult);
                await _courseResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await SendStudentCompleteCourse(studentId, courseId, courseResult, cancellationToken);
            }
        }

        private async Task SendStudentCompleteCourse(Guid studentId, Guid courseId, CourseResult courseResult, CancellationToken cancellationToken)
        {
            var studentResult = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { studentId });
            var student = studentResult.Content?.Result?.FirstOrDefault();
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
    }
}
