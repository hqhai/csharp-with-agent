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
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly FinishOneUnitPublisher _finishOneUnitPublisher;
        private readonly FinishOneLevelPassPublisher _finishOneLevelPassPublisher;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;

        public BaseInternalEventHandler(IVideoResultRepository videoResultRepository,
            IClassForumResultRepository classForumResultRepository,
            IUnitResultRepository unitResultRepository,
            ILessonResultRepository lessonResultRepository,
            ICourseResultRepository courseResultRepository,
            ICourseRepository courseRepository,
            IUnitRepository unitRepository,
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
            _finishOneUnitPublisher = finishOneUnitPublisher;
            _finishOneLevelPassPublisher = finishOneLevelPassPublisher;
            _finalTestResultRepository = finalTestResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
        }

        #region Get Skill Scores Unit

        public async Task<(List<SkillScores>, double)> UpdateUnitResult(IList<Guid>? lessonResultIds)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            var (videoSkillScores, percentVideo) = await VideoSkillScores(lessonResultIds, EnumTimeCodeType.Standalone, 18);
            var (unitTestSkillScores, percentUnitTest) = await VideoSkillScores(lessonResultIds, EnumTimeCodeType.UnitTest, 30);
            var (skillTestSkillScores, percentSkillTest) = await VideoSkillScores(lessonResultIds, EnumTimeCodeType.SkillTest, 10);
            var (homeWorkSkillScores, percentHomeWork) = await HomeWordsSkillScores(lessonResultIds, 22);
            var (classForumSkillScores, percentClassForum) = await ClassForumSkillScores(lessonResultIds, 20);
            List<SkillScores> mergedSkillScores = videoSkillScores.Concat(homeWorkSkillScores)
                                                    .Concat(classForumSkillScores)
                                                    .Concat(skillTestSkillScores)
                                                    .Concat(unitTestSkillScores)
                                                    .ToList();
            List<SkillScores> groupedSkillScores = mergedSkillScores
                                .GroupBy(x => x.Skill)
                                .Select(group => GetSumScoreByCourse(group)).ToList();
            var percent = percentClassForum + percentHomeWork + percentSkillTest + percentUnitTest + percentVideo;
            return (groupedSkillScores, percent);
        }

        public async Task<(List<SkillScores>, double)> VideoSkillScores(IList<Guid>? lessonResultIds, EnumTimeCodeType type, int percentSkill)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            List<SkillScores> skillScores = new List<SkillScores>();
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId));
            if (videoResult != null && videoResult.VideoSkillScores != null)
            {
                skillScores = videoResult.VideoSkillScores.Where(x => x.Type == type && x.SkillScores != null && x.SkillScores.Any())
                                        .SelectMany(x => x.SkillScores!).GroupBy(x => x.Skill).Select(x => GetSumScoreByCourse(x)).ToList();
                return (skillScores, skillScores.Sum(x => x.Percent * percentSkill / skillScores.Count));
            }
            return (skillScores, 0);
        }

        public async Task<(List<SkillScores>, double)> ClassForumSkillScores(IList<Guid>? lessonResultIds, int percentSkill)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            List<SkillScores> skillScores = new List<SkillScores>();
            var classForumResults = await _classForumResultRepository.Queryable.Include(x => x.ClassForum)
                .Include(x => x.ClassForumScores)
                .Where(x => x.Status == EnumClassForumResultStatus.Graded && lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
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
                    skillScore.Percent = x.ClassForumScores.Sum(x => x.Score) / 36;
                    return skillScore;
                }).ToList();
                skillScores = skillScores.GroupBy(x => x.Skill).Select(x => GetSumScoreByCourse(x)).ToList();
                return (skillScores, skillScores.Average(x => x.Percent * (percentSkill * skillScores.Count)));
            }
            return (skillScores, 0);
        }

        public async Task<(List<SkillScores>, double)> HomeWordsSkillScores(IList<Guid>? lessonResultIds, int percentSkill)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            List<SkillScores> skillScores = new List<SkillScores>();
            var homeWorkResults = await _homeWorkResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
            if (homeWorkResults != null)
            {
                skillScores = homeWorkResults.Where(x => x.SkillScores != null && x.SkillScores.Any())
                                        .SelectMany(x => x.SkillScores!)
                                        .GroupBy(x => x.Skill).Select(x => GetSumScoreByCourse(x)).ToList();
            }
            return (skillScores, skillScores.Sum(x => x.Percent * percentSkill / skillScores.Count));
        }

        #endregion Get Skill Scores Unit

        #region Get Skill Scores Course

        public async Task<(List<SkillScores>, double)> UpdateCourseResult(IList<Guid> unitIds, Guid studentId, EnumCourseType type, Guid? finalTestId)
        {
            ArgumentNullException.ThrowIfNull(unitIds);
            var units = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.StudentId == studentId)).Where(x => unitIds.Contains(x.Id)).ToListAsync();
            var lessonResults = units.SelectMany(x => x.LessonResults).Where(x => x.StudentId == studentId).ToList();

            var (videoSkillScores, percentVideo) = await VideoSkillScoreByCourses(lessonResults.Select(x => x.Id).ToList(), type);
            var (homeWorkSkillScores, percentHomeWork) = await HomeWorkSkillScoreByCourses(lessonResults.Select(x => x.Id).ToList(), type);
            var (classForumSkillScores, percentClassForum) = await ClassForumSkillScoreByCourses(lessonResults.Select(x => x.Id).ToList(), type);
            double percent = percentClassForum + percentHomeWork + percentVideo;

            List<SkillScores> mergedSkillScores = videoSkillScores
                                               .Concat(homeWorkSkillScores)
                                               .Concat(classForumSkillScores)
                                               .ToList();
            if (type == EnumCourseType.Academic)
            {
                var (unitSkillScores, percentUnitSkill) = await SkillScoreByCourses(unitIds, studentId, EnumTimeCodeType.UnitTest);
                var (skillSkillScores, percentSkill) = await SkillScoreByCourses(unitIds, studentId, EnumTimeCodeType.SkillTest);
                var (finalTestSkillScores, percentFinalTest) = await FinalTestSkillScoreByCourses(finalTestId, studentId);
                mergedSkillScores = mergedSkillScores.Concat(skillSkillScores)
                                                   .Concat(unitSkillScores)
                                                   .Concat(finalTestSkillScores)
                                                   .ToList();
                percent = percent + percentUnitSkill + percentSkill + percentFinalTest;
            }
            List<SkillScores> groupedSkillScores = mergedSkillScores
                               .GroupBy(x => x.Skill)
                               .Select(group => GetSumScoreByCourse(group)).ToList();
            return (groupedSkillScores, percent);
        }

        public async Task<(List<SkillScores>, double)> VideoSkillScoreByCourses(IList<Guid>? lessonResultIds, EnumCourseType type)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);

            List<SkillScores> skillScores = new List<SkillScores>();
            var videoResults = await _videoResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
            if (videoResults != null)
            {
                skillScores = videoResults.Where(x => x.VideoSkillScores != null && x.VideoSkillScores.Any())
                                        .SelectMany(x => x.VideoSkillScores!)
                                        .Where(x => x.Type == EnumTimeCodeType.Standalone && x.SkillScores != null && x.SkillScores.Any())
                                        .SelectMany(x => x.SkillScores!).GroupBy(x => x.Skill).Select(x => GetSumScoreByCourse(x)).ToList();
                if (type == EnumCourseType.Academic)
                {
                    return (skillScores, skillScores.Average(x => x.Percent * ((double)9 * skillScores.Count)));
                }
                return (skillScores, skillScores.Average(x =>
                {
                    double percent = 0;
                    if (x.Skill == EnumCourseSkill.Writing || x.Skill == EnumCourseSkill.Speaking)
                    {
                        percent = x.Percent * 3.5;
                    }
                    else
                    {
                        percent = x.Percent * 3.25;
                    }
                    return percent;
                }));
            }
            return (skillScores, 0);
        }

        public SkillScores GetSumScoreByCourse(IGrouping<EnumCourseSkill, SkillScores>? group)
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
                    Percent = group.Average(x => x.Percent),
                };
            }
            return new SkillScores();
        }

        public SkillScores GetSkillSkillScore(IGrouping<EnumCourseSkill, SkillScores>? x)
        {
            if (x != null)
            {
                SkillScores skillScores = new SkillScores();
                skillScores.Skill = x.Key;
                skillScores.Scores = x.Sum(x => x.Scores) > 0 ? x.Average(x => x.Scores) : default;
                skillScores.TotalQuestion = x.Sum(x => x.TotalQuestion);
                skillScores.CountQuestion = x.Sum(x => x.CountQuestion);
                skillScores.TotalCount = x.Sum(x => x.TotalCount);
                skillScores.CorrectCount = x.Sum(x => x.CorrectCount);
                skillScores.Percent = x.Sum(x => x.CorrectCount) / x.Sum(x => x.TotalCount);
                return skillScores;
            };
            return new SkillScores();
        }

        public async Task<(List<SkillScores>, double)> HomeWorkSkillScoreByCourses(IList<Guid>? lessonResultIds, EnumCourseType type)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);

            List<SkillScores> skillScores = new List<SkillScores>();
            var homeWorkResults = await _homeWorkResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
            if (homeWorkResults != null)
            {
                skillScores = homeWorkResults.Where(x => x.SkillScores != null && x.SkillScores.Any())
                                        .SelectMany(x => x.SkillScores!)
                                        .GroupBy(x => x.Skill).Select(x => GetSumScoreByCourse(x)).ToList();
                if (type == EnumCourseType.Academic)
                {
                    return (skillScores, skillScores.Average(x => x.Percent * ((double)14 * skillScores.Count)));
                }
                return (skillScores, skillScores.Average(x => x.Percent * ((double)48 * skillScores.Count)));
            }
            return (skillScores, 0);
        }

        public async Task<(List<SkillScores>, double)> ClassForumSkillScoreByCourses(IList<Guid>? lessonResultIds, EnumCourseType type)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);

            List<SkillScores> skillScores = new List<SkillScores>();
            var classForumResults = await _classForumResultRepository.Queryable.Include(x => x.ClassForumScores).Include(x => x.ClassForum)
                                .Where(x => x.Status == EnumClassForumResultStatus.Graded && lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
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
                    skillScore.Percent = x.ClassForumScores.Sum(x => x.Score) / 36;
                    return skillScore;
                }).ToList();

                skillScores = skillScores.GroupBy(x => x.Skill).Select(x => GetSumScoreByCourse(x)).ToList();

                if (type == EnumCourseType.Academic)
                {
                    return (skillScores, skillScores.Average(x => x.Percent * (20 * skillScores.Count)));
                }
                return (skillScores, skillScores.Average(x => x.Percent * (32 * skillScores.Count)));
            }
            return (skillScores, 0);
        }

        public async Task<(List<SkillScores>, double)> SkillScoreByCourses(IList<Guid>? unitIds, Guid studentId, EnumTimeCodeType type)
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
                    var skillScores = videoResults.Where(x => x.VideoSkillScores != null && x.VideoSkillScores.Any())
                                        .SelectMany(x => x.VideoSkillScores!)
                                        .Where(x => x.Type == type && x.SkillScores != null && x.SkillScores.Any())
                                        .SelectMany(x => x.SkillScores!)
                                        .GroupBy(x => x.Skill)
                                        .Select(x => GetSumScoreByCourse(x)).ToList();
                    var percent = skillScores.Average(x => x.Percent) * 24 / unitIds.Count;
                    skillScorePercents.Add((skillScores, percent));
                }
            }
            var skillScoreSkills = skillScorePercents.SelectMany(x => x.Item1).GroupBy(x => x.Skill).Select(x => GetSumScoreByCourse(x)).ToList();
            return (skillScoreSkills, skillScorePercents.Sum(x => x.Item2));
        }

        public async Task<(List<SkillScores>, double)> FinalTestSkillScoreByCourses(Guid? finalTestId, Guid studentId)
        {
            ArgumentNullException.ThrowIfNull(finalTestId);
            var finalTestResult = await _finalTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.FinalTestId == finalTestId && x.StudentId == studentId && x.Status == EnumResultStatus.Done);
            var skillScores = finalTestResult?.SkillScores?.GroupBy(x => x.Skill).Select(x => GetSumScoreByCourse(x)).ToList();
            var percent = finalTestResult?.Percent * 15;
            return (skillScores ?? new List<SkillScores>(), percent ?? default);
        }

        #endregion Get Skill Scores Course

        #region Update Unit

        public async Task UpdateUnit(IList<Guid>? lessonResultIds, Domain.Entities.Unit? unit, Guid courseId, Guid studentId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            ArgumentNullException.ThrowIfNull(unit);
            var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == unit.Id && x.StudentId == studentId && x.CourseId == courseId, cancellationToken);
            if (unit != null && unitResult != null)
            {
                var (groupedSkillScores, percent) = await UpdateUnitResult(lessonResultIds);
                unitResult.CorrectCount = (int)groupedSkillScores.Sum(x => x.CorrectCount);
                unitResult.CorrectTotal = (int)groupedSkillScores.Sum(x => x.TotalCount);
                unitResult.Status = EnumResultStatus.Done;
                unitResult.Percent = percent;
                unitResult.SkillScores = groupedSkillScores;
                await _finishOneUnitPublisher.Publish(unitResult, cancellationToken);
                _unitResultRepository.Update(unitResult);
                await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        #endregion Update Unit

        #region Update Status Same Level Lesson

        public async Task UpdateTheNextLesson(Unit? unit, LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(unit);
            var mockTestId = unit.UnitSkillMockTests.FirstOrDefault()?.MockTestId;
            var displayOrder = unit.UnitLessons.FirstOrDefault(x => x.LessonId == lessonResult.LessonId)!.DisplayOrder;
            var lesson = unit.UnitLessons.FirstOrDefault(x => x.DisplayOrder == displayOrder + 1)?.Lesson;
            if (lesson != null)
            {
                var lessonResultNext = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == lessonResult.CourseId && x.UnitId == lessonResult.UnitId && x.StudentId == lessonResult.StudentId && x.LessonId == lesson.Id, cancellationToken);
                if (lessonResultNext != null)
                {
                    lessonResultNext.Status = EnumResultStatus.New;
                    _lessonResultRepository.Update(lessonResultNext);
                    await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            else if (unit.CourseLevel.GetEnumCourseType() == EnumCourseType.Ielts && mockTestId.HasValue)
            {
                var mockTestResult = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == lessonResult.CourseId && x.UnitId == lessonResult.UnitId && x.StudentId == lessonResult.StudentId && x.MockTestId == mockTestId.Value, cancellationToken);
                if (mockTestResult != null)
                {
                    mockTestResult.Status = EnumResultStatus.New;
                    _mockTestResultRepository.Update(mockTestResult);
                    await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        #endregion Update Status Same Level Lesson

        #region Update Status Same Level Unit

        public async Task UpdateProcessUnit(UnitResult? unitResult, CancellationToken cancellationToken)
        {
            var courseId = unitResult != null ? unitResult.CourseId : default;
            if (courseId != default && unitResult != null)
            {
                var course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests)
                                                  .Include(x => x.UnitResults)
                                                  .Include(x => x.MockTestResults)
                                                  .Include(x => x.FinalTestResults)
                                                  .FirstOrDefaultAsync(x => x.Id == courseId, cancellationToken);
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
                var course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests)
                                                  .Include(x => x.UnitResults)
                                                  .Include(x => x.MockTestResults)
                                                  .Include(x => x.FinalTestResults)
                                                  .FirstOrDefaultAsync(x => x.Id == courseId, cancellationToken);
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

        public async Task UpdateStatusProcess(CourseUnitMockTest? courseUnitMockTest, Guid studentId, CancellationToken cancellationToken)
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
                        await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    break;

                case var value when value == (courseUnitMockTest.FinalTestId == null):
                    var finalTestResultNext = await _finalTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.FinalTestId == courseUnitMockTest.FinalTestId, cancellationToken);
                    if (finalTestResultNext != null)
                    {
                        finalTestResultNext.Status = EnumResultStatus.New;
                        _finalTestResultRepository.Update(finalTestResultNext);
                        await _finalTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    break;

                case var value when value == (courseUnitMockTest.MockTestId == null):
                    var mockTestResultNext = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.MockTestId == courseUnitMockTest.MockTestId, cancellationToken);
                    if (mockTestResultNext != null)
                    {
                        mockTestResultNext.Status = EnumResultStatus.New;
                        _mockTestResultRepository.Update(mockTestResultNext);
                        await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    break;

                default:
                    break;
            }
        }

        #endregion Update Status Same Level Unit

        #region Update Course

        public async Task UpdateCourse(Guid courseId, Guid studentId)
        {
            var course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests).FirstOrDefaultAsync(x => x.Id == courseId);
            if (course != null)
            {
                var finalTestId = course.CourseUnitMockTests.Where(x => x.FinalTestId != null).FirstOrDefault()?.FinalTestId;
                var unitIds = course.CourseUnitMockTests.Where(x => x.UnitId != null).Select(x => x.UnitId ?? default).ToList();
                var courseType = course.CourseLevel.GetEnumCourseType();
                await UpdateCourseResult(unitIds, studentId, courseType, finalTestId);
            }
        }

        public async Task UpdateCourseResult(Guid courseId, Guid studentId, CancellationToken cancellationToken)
        {
            await UpdateCourse(courseId, studentId);
            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == courseId && x.StudentId == studentId, cancellationToken);
            if (courseResult != null)
            {
                await _finishOneLevelPassPublisher.Publish(courseResult, cancellationToken);
                courseResult.Status = EnumCourseStatus.InActive;
                _courseResultRepository.Update(courseResult);
                await _courseResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        #endregion Update Course
    }
}
