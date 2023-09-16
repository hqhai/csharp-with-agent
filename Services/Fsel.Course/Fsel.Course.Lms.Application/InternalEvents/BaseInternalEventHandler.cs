// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Linq;
    using System.Threading;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public class BaseInternalEventHandler
    {
        protected readonly IVideoResultRepository _videoResultRepository;
        protected readonly IClassForumResultRepository _classForumResultRepository;
        protected readonly IUnitResultRepository _unitResultRepository;
        protected readonly ILessonResultRepository _lessonResultRepository;
        protected readonly ICourseResultRepository _courseResultRepository;
        protected readonly ICourseRepository _courseRepository;
        protected readonly IUnitRepository _unitRepository;
        protected readonly IMockTestRepository _mockTestRepository;
        protected readonly IHomeWorkQuestionRepository _homeWorkQuestionRepository;
        protected readonly IHomeWorkAnswerRepository _homeWorkAnswerRepository;
        protected readonly IQuestionRepository _questionRepository;
        protected readonly IHomeWorkRepository _homeWorkRepository;
        protected readonly FinishOneUnitPublisher _finishOneUnitPublisher;
        protected readonly FinishOneLevelPassPublisher _finishOneLevelPassPublisher;
        protected readonly IFinalTestResultRepository _finalTestResultRepository;
        protected readonly IMockTestResultRepository _mockTestResultRepository;
        protected readonly IHomeWorkResultRepository _homeWorkResultRepository;

        public BaseInternalEventHandler(IVideoResultRepository videoResultRepository,
            IClassForumResultRepository classForumResultRepository,
            IUnitResultRepository unitResultRepository,
            ILessonResultRepository lessonResultRepository,
            ICourseResultRepository courseResultRepository,
            ICourseRepository courseRepository,
            IUnitRepository unitRepository,
            IMockTestRepository mockTestRepository,
            IHomeWorkQuestionRepository homeWorkQuestionRepository,
            IHomeWorkAnswerRepository homeWorkAnswerRepository,
            IQuestionRepository questionRepository,
            IHomeWorkRepository homeWorkRepository,
            FinishOneUnitPublisher finishOneUnitPublisher,
            FinishOneLevelPassPublisher finishOneLevelPassPublisher,
            IFinalTestResultRepository finalTestResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository)
        {
            _videoResultRepository = videoResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _mockTestRepository = mockTestRepository;
            _homeWorkQuestionRepository = homeWorkQuestionRepository;
            _homeWorkAnswerRepository = homeWorkAnswerRepository;
            _questionRepository = questionRepository;
            _homeWorkRepository = homeWorkRepository;
            _finishOneUnitPublisher = finishOneUnitPublisher;
            _finishOneLevelPassPublisher = finishOneLevelPassPublisher;
            _finalTestResultRepository = finalTestResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
        }

        private async Task<(List<SkillScores>, double)> GetCourseResult(IList<Guid> unitIds, Guid studentId, EnumCourseType type, Guid? finalTestId)
        {
            ArgumentNullException.ThrowIfNull(unitIds);
            var units = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.StudentId == studentId)).Where(x => unitIds.Contains(x.Id)).ToListAsync();
            var lessonResults = units.SelectMany(x => x.LessonResults).Where(x => x.StudentId == studentId).ToList();
            var (videoSkillScores, percentVideo) = await GetVideoSkillScores(lessonResults.Select(x => x.Id).ToList(), EnumTimeCodeType.Standalone, 0, type);
            var (homeWorkSkillScores, percentHomeWork) = await GetHomeWordsSkillScores(lessonResults.Select(x => x.Id).ToList(), 0, type);
            var (classForumSkillScores, percentClassForum) = await GetClassForumSkillScores(lessonResults.Select(x => x.Id).ToList(), 0, type);
            double percent = percentClassForum + percentHomeWork + percentVideo;
            List<SkillScores> mergedSkillScores = videoSkillScores.Concat(homeWorkSkillScores).Concat(classForumSkillScores).ToList();
            if (type == EnumCourseType.Academic)
            {
                var (unitSkillScores, percentUnitSkill) = await GetSkillScoreByCourses(unitIds, studentId, EnumTimeCodeType.UnitTest);
                var (skillSkillScores, percentSkill) = await GetSkillScoreByCourses(unitIds, studentId, EnumTimeCodeType.SkillTest);
                var (finalTestSkillScores, percentFinalTest) = await GetFinalTestSkillScore(finalTestId, studentId);
                mergedSkillScores = mergedSkillScores.Concat(skillSkillScores).Concat(unitSkillScores).Concat(finalTestSkillScores).ToList();
                percent = percent + percentUnitSkill + percentSkill + percentFinalTest;
            }
            List<SkillScores> groupedSkillScores = mergedSkillScores.GroupBy(x => x.Skill).Select(group => GetSumSkillScore(group)).ToList();
            return (groupedSkillScores, percent);
        }

        public async Task<(List<SkillScores>, double)> GetVideoSkillScores(IList<Guid>? lessonResultIds, EnumTimeCodeType type, int percentSkill = 0, EnumCourseType? courseType = null)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            List<SkillScores> skillScores = new List<SkillScores>();
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId));
            if (videoResult != null && videoResult.VideoSkillScores != null)
            {
                skillScores = videoResult.VideoSkillScores.Where(x => x.Type == type && x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!).GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList();
                if (courseType == EnumCourseType.Academic)
                {
                    return (skillScores, skillScores.Any() ? NumberHelper.ConvertDoublePercent(skillScores.Average(x => x.Percent * ((double)9 * skillScores.Count))) : default);
                }
                else if (courseType != null)
                {
                    return (skillScores, skillScores.Any() ? NumberHelper.ConvertDoublePercent(skillScores.Average(x =>
                    {
                        if (x.Skill == EnumCourseSkill.Writing || x.Skill == EnumCourseSkill.Speaking)
                        {
                            return NumberHelper.ConvertDoublePercent(x.Percent * 3.5);
                        }
                        return NumberHelper.ConvertDoublePercent(x.Percent * 3.25);
                    })) : default);
                }
            }
            return (skillScores, skillScores.Any() ? NumberHelper.ConvertDoublePercent(skillScores.Sum(x => x.Percent * percentSkill / skillScores.Count)) : default);
        }

        public async Task<(List<SkillScores>, double)> GetClassForumSkillScores(IList<Guid>? lessonResultIds, int percentSkill = 0, EnumCourseType? courseType = null)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            List<SkillScores> skillScores = new List<SkillScores>();
            var classForumResults = await _classForumResultRepository.Queryable.Include(x => x.ClassForum).Include(x => x.ClassForumScores).Where(x => x.Status == EnumClassForumResultStatus.Graded && lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
            if (classForumResults != null)
            {
                skillScores = classForumResults.Select(x =>
                {
                    SkillScores skillScore = new SkillScores();
                    skillScore.Skill = x.ClassForum!.CourseSkill;
                    skillScore.TotalQuestion = 1;
                    skillScore.CountQuestion = 1;
                    skillScore.TotalCount = 36;
                    skillScore.CorrectCount = x.ClassForumScores.Sum(x => x.Score);
                    skillScore.Percent = NumberHelper.ConvertDouble(x.ClassForumScores.Sum(x => x.Score) / 36 * 100);
                    return skillScore;
                }).ToList().GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList();
                if (courseType != null && courseType == EnumCourseType.Academic)
                {
                    return (skillScores, skillScores.Any() ? NumberHelper.ConvertDoublePercent(skillScores.Average(x => x.Percent * (20 * skillScores.Count))) : default);
                }
                else if (courseType != null && courseType == EnumCourseType.Ielts)
                {
                    return (skillScores, skillScores.Any() ? NumberHelper.ConvertDoublePercent(skillScores.Average(x => x.Percent * (32 * skillScores.Count))) : default);
                }
            }
            return (skillScores, skillScores.Any() ? NumberHelper.ConvertDouble(skillScores.Average(x => x.Percent * (percentSkill * skillScores.Count))) : default);
        }

        public async Task<(List<SkillScores>, double)> GetHomeWordsSkillScores(IList<Guid>? lessonResultIds, int percentSkill = 0, EnumCourseType? courseType = null)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            List<SkillScores> skillScores = new List<SkillScores>();
            var homeWorkResults = await _homeWorkResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
            if (homeWorkResults != null)
            {
                skillScores = homeWorkResults.Where(x => x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!).GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList();
                if (courseType != null && courseType == EnumCourseType.Academic)
                {
                    return (skillScores, NumberHelper.ConvertDoublePercent(skillScores.Average(x => x.Percent * ((double)14 * skillScores.Count))));
                }
                else if (courseType != null && courseType == EnumCourseType.Academic)
                {
                    return (skillScores, NumberHelper.ConvertDoublePercent(skillScores.Average(x => x.Percent * ((double)48 * skillScores.Count))));
                }
            }
            return (skillScores, skillScores.Any() ? NumberHelper.ConvertDoublePercent(skillScores.Sum(x => x.Percent * percentSkill / skillScores.Count)) : default);
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
                    Percent = NumberHelper.ConvertDouble(group.Average(x => x.Percent)),
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
                skillScores.Percent = NumberHelper.ConvertDouble(x.Sum(x => x.CorrectCount) / x.Sum(x => x.TotalCount) * 100);
                return skillScores;
            };
            return skillScores;
        }

        private async Task<(List<SkillScores>, double)> GetSkillScoreByCourses(IList<Guid>? unitIds, Guid studentId, EnumTimeCodeType type)
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
                        var percent = skillScores.Any() ? NumberHelper.ConvertDoublePercent(skillScores.Average(x => x.Percent) * 24 / unitIds.Count) : default;
                        skillScorePercents.Add((skillScores, percent));
                    }
                }
            }
            if (skillScorePercents.Any())
            {
                var skillScoreSkills = skillScorePercents.SelectMany(x => x.Item1).GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList();
                return (skillScoreSkills, NumberHelper.ConvertDouble(skillScorePercents.Average(x => x.Item2)));
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
            var percent = NumberHelper.ConvertDoublePercent(finalTestResult?.Percent * 15 ?? default);
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
                    var displayOrder = course.CourseUnitMockTests.FirstOrDefault(x => x.UnitId == unitResult.UnitId)?.DisplayOrder;
                    var courseUnitMockTest = course.CourseUnitMockTests.FirstOrDefault(x => x.DisplayOrder == displayOrder + 1);
                    await UpdateStatusProcess(courseUnitMockTest, unitResult.StudentId, cancellationToken);
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
                    courseResult.Percent = NumberHelper.ConvertDouble(percent);
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
                courseResult.Status = EnumCourseStatus.InActive;
                _courseResultRepository.Update(courseResult);
                await _courseResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
