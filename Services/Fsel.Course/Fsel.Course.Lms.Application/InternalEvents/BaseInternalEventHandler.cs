// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
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

        public async Task<(List<SkillScores>, double)> UpdateUnitResult(IList<LessonResult> lessonResults)
        {
            ArgumentNullException.ThrowIfNull(lessonResults);
            List<SkillScores> videoSkillScores = new List<SkillScores>();
            List<SkillScores> homeSkillScores = new List<SkillScores>();
            List<SkillScores> classForumSkillScores = new List<SkillScores>();
            List<SkillScores> skillTestSkillScores = new List<SkillScores>();
            List<SkillScores> unitTestSkillScores = new List<SkillScores>();
            foreach (var item in lessonResults)
            {
                videoSkillScores.AddRange(await VideoSkillScores(item.Id));
                unitTestSkillScores.AddRange(await UnitTestSkillScores(item.Id));
                skillTestSkillScores.AddRange(await SkillTestSkillScores(item.Id));
                homeSkillScores.AddRange(await HomeWordsSkillScores(item.Id));
                var classForumSkillScore = await ClassForumSkillScores(item.Id);
                if (classForumSkillScore != null)
                {
                    classForumSkillScores.Add(classForumSkillScore);
                }
            }

            List<SkillScores> mergedSkillScores = videoSkillScores
                                                    .Concat(homeSkillScores)
                                                    .Concat(classForumSkillScores)
                                                    .Concat(skillTestSkillScores)
                                                    .Concat(unitTestSkillScores)
                                                    .ToList();
            List<SkillScores> groupedSkillScores = mergedSkillScores
                                .GroupBy(x => x.Skill)
                                .Select(group => new SkillScores
                                {
                                    Skill = group.Key,
                                    Scores = group.Average(x => x.Scores),
                                    TotalCount = group.Sum(x => x.TotalCount),
                                    CorrectCount = group.Sum(x => x.CorrectCount),
                                    CountQuestion = group.Sum(x => x.CountQuestion),
                                    TotalQuestion = group.Sum(x => x.TotalQuestion),
                                    Percent = group.Average(x => x.Percent),
                                }).ToList();
            var percent = await PercentUnit(videoSkillScores, 18) + await PercentUnit(homeSkillScores, 22) + await PercentUnit(classForumSkillScores, 20) + await PercentUnit(skillTestSkillScores, 10) + await PercentUnit(unitTestSkillScores, 30);
            return (groupedSkillScores, percent);
        }

        public async Task<List<SkillScores>> VideoSkillScores(Guid? lessonResultId)
        {
            ArgumentNullException.ThrowIfNull(lessonResultId);
            List<SkillScores> skillScores = new List<SkillScores>();
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.Status == EnumResultStatus.Done && x.LessonResultId == lessonResultId);
            if (videoResult != null && videoResult.VideoSkillScores != null)
            {
                skillScores = videoResult.VideoSkillScores.Where(x => x.Type == EnumTimeCodeType.Standalone).SelectMany(x => x.SkillScores!).Where(x => x.TotalCount != 0).ToList();
            }
            return skillScores;
        }

        public async Task<List<SkillScores>> UnitTestSkillScores(Guid? lessonResultId)
        {
            ArgumentNullException.ThrowIfNull(lessonResultId);
            List<SkillScores> skillScores = new List<SkillScores>();
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.Status == EnumResultStatus.Done && x.LessonResultId == lessonResultId);
            if (videoResult != null && videoResult.VideoSkillScores != null)
            {
                skillScores = videoResult.VideoSkillScores.Where(x => x.Type == EnumTimeCodeType.UnitTest).SelectMany(x => x.SkillScores!).Where(x => x.TotalCount != 0).ToList();
            }
            return skillScores;
        }

        public async Task<List<SkillScores>> SkillTestSkillScores(Guid? lessonResultId)
        {
            ArgumentNullException.ThrowIfNull(lessonResultId);
            List<SkillScores> skillScores = new List<SkillScores>();
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.Status == EnumResultStatus.Done && x.LessonResultId == lessonResultId);
            if (videoResult != null && videoResult.VideoSkillScores != null)
            {
                skillScores = videoResult.VideoSkillScores.Where(x => x.Type == EnumTimeCodeType.SkillTest).SelectMany(x => x.SkillScores!).Where(x => x.TotalCount != 0).ToList();
            }
            return skillScores;
        }

        public async Task<SkillScores?> ClassForumSkillScores(Guid? lessonResultId)
        {
            ArgumentNullException.ThrowIfNull(lessonResultId);
            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForum).Include(x => x.ClassForumScores).FirstOrDefaultAsync(x => x.Status == EnumClassForumResultStatus.Graded && x.LessonResultId == lessonResultId);
            if (classForumResult != null && classForumResult.ClassForumScores != null && classForumResult.ClassForum != null)
            {
                SkillScores skillScores = new SkillScores();
                skillScores.Skill = classForumResult.ClassForum.CourseSkill;
                skillScores.TotalCount = 36;
                skillScores.CorrectCount = classForumResult.ClassForumScores.Sum(x => x.Score);
                return skillScores;
            }
            return null;
        }

        public async Task<List<SkillScores>> HomeWordsSkillScores(Guid? lessonResultId)
        {
            ArgumentNullException.ThrowIfNull(lessonResultId);
            List<SkillScores> skillScores = new List<SkillScores>();
            var homeWorkResults = await _homeWorkResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.LessonResultId == lessonResultId).ToListAsync();
            if (homeWorkResults != null)
            {
                foreach (var item in homeWorkResults)
                {
                    if (item.SkillScores != null)
                    {
                        skillScores.AddRange(item.SkillScores.Where(x => x.TotalCount != 0 && x.CorrectCount != 0).ToList());
                    }
                }
            }
            return skillScores;
        }

        #endregion Get Skill Scores Unit

        #region Get Skill Scores Course

        public async Task<(List<SkillScores>, double)> UpdateCourseAcademic(IList<Guid> unitIds, Guid studentId)
        {
            ArgumentNullException.ThrowIfNull(unitIds);
            List<SkillScores> videoSkillScores = new List<SkillScores>();
            List<SkillScores> homeSkillScores = new List<SkillScores>();
            List<SkillScores> classForumSkillScores = new List<SkillScores>();
            List<SkillScores> skillTestSkillScores = new List<SkillScores>();
            List<SkillScores> unitTestSkillScores = new List<SkillScores>();

            foreach (var item in unitIds)
            {
                var unit = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.StudentId == studentId)).FirstOrDefaultAsync(x => x.Id == item);
                var lessonResultIds = unit?.LessonResults.Where(x => x.StudentId == studentId).Select(x => x.Id).ToList();
                videoSkillScores.AddRange(await VideoSkillScoreByCourses(lessonResultIds, EnumTimeCodeType.Standalone));
                unitTestSkillScores.AddRange(await VideoSkillScoreByCourses(lessonResultIds, EnumTimeCodeType.UnitTest));
                skillTestSkillScores.AddRange(await VideoSkillScoreByCourses(lessonResultIds, EnumTimeCodeType.SkillTest));
                homeSkillScores.AddRange(await HomeWordsSkillScoreByCourses(item));
                var classForumSkillScore = await ClassForumSkillScoreByCourses(item);
                if (classForumSkillScore != null)
                {
                    classForumSkillScores.Add(classForumSkillScore);
                }
            }

            List<SkillScores> mergedSkillScores = videoSkillScores
                                                    .Concat(homeSkillScores)
                                                    .Concat(classForumSkillScores)
                                                    .Concat(skillTestSkillScores)
                                                    .Concat(unitTestSkillScores)
                                                    .ToList();
            List<SkillScores> groupedSkillScores = mergedSkillScores
                                .GroupBy(x => x.Skill)
                                .Select(group => new SkillScores
                                {
                                    Skill = group.Key,
                                    Scores = group.Average(x => x.Scores),
                                    TotalCount = group.Sum(x => x.TotalCount),
                                    CorrectCount = group.Sum(x => x.CorrectCount),
                                    CountQuestion = group.Sum(x => x.CountQuestion),
                                    TotalQuestion = group.Sum(x => x.TotalQuestion),
                                    Percent = group.Average(x => x.Percent),
                                }).ToList();
            var percent = await PercentUnit(videoSkillScores, 18) + await PercentUnit(homeSkillScores, 22) + await PercentUnit(classForumSkillScores, 20) + await PercentUnit(skillTestSkillScores, 10) + await PercentUnit(unitTestSkillScores, 30);
            return (groupedSkillScores, percent);
        }

        public async Task<List<SkillScores>> VideoSkillScoreByCourses(IList<Guid>? lessonResultIds, EnumTimeCodeType type)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            List<SkillScores> skillScores = new List<SkillScores>();
            var videoResults = await _videoResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
            if (videoResults != null)
            {
                skillScores = videoResults.Where(x => x.VideoSkillScores != null && x.VideoSkillScores.Any())
                                        .SelectMany(x => x.VideoSkillScores!)
                                        .Where(x => x.Type == type && x.SkillScores != null && x.SkillScores.Any())
                                        .SelectMany(x => x.SkillScores!)
                                        .ToList();
            }
            return skillScores;
        }

        public async Task<SkillScores?> ClassForumSkillScoreByCourses(Guid? lessonResultId)
        {
            ArgumentNullException.ThrowIfNull(lessonResultId);
            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForum).Include(x => x.ClassForumScores).FirstOrDefaultAsync(x => x.Status == EnumClassForumResultStatus.Graded && x.LessonResultId == lessonResultId);
            if (classForumResult != null && classForumResult.ClassForumScores != null && classForumResult.ClassForum != null)
            {
                SkillScores skillScores = new SkillScores();
                skillScores.Skill = classForumResult.ClassForum.CourseSkill;
                skillScores.TotalCount = 36;
                skillScores.CorrectCount = classForumResult.ClassForumScores.Sum(x => x.Score);
                return skillScores;
            }
            return null;
        }

        public async Task<List<SkillScores>> HomeWordsSkillScoreByCourses(Guid? lessonResultId)
        {
            ArgumentNullException.ThrowIfNull(lessonResultId);
            List<SkillScores> skillScores = new List<SkillScores>();
            var homeWorkResults = await _homeWorkResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.LessonResultId == lessonResultId).ToListAsync();
            if (homeWorkResults != null)
            {
                foreach (var item in homeWorkResults)
                {
                    if (item.SkillScores != null)
                    {
                        skillScores.AddRange(item.SkillScores.Where(x => x.TotalCount != 0 && x.CorrectCount != 0).ToList());
                    }
                }
            }
            return skillScores;
        }

        #endregion Get Skill Scores Course

        #region Update Course

        public async Task UpdateCourse(Guid courseId, Guid studentId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course != null)
            {
                var courseResult = await _courseResultRepository.GetByIdAsync(courseId);

                var courseType = course.CourseLevel.GetEnumCourseType();
                //if (courseType == EnumCourseType.Academic)
                //{
                //    UpdateCourseAcademic(lessonResults, courseResult);
                //}
                //else
                //{
                //    UpdateCourseAcademic(lessonResults, courseResult);
                //}
            }
        }

        #endregion Update Course

        #region Tinh Diem Unit

        public Task<double> PercentUnit(IList<SkillScores>? skillScores, int percentSkill)
        {
            ArgumentNullException.ThrowIfNull(skillScores);
            double percent = 0;
            List<SkillScores> unitkillScores = skillScores
                                     .GroupBy(x => x.Skill)
                                     .Select(group => new SkillScores
                                     {
                                         Skill = group.Key,
                                         Scores = group.Sum(x => x.Scores),
                                         TotalCount = group.Sum(x => x.TotalCount),
                                         CorrectCount = group.Sum(x => x.CorrectCount)
                                     })
                                     .ToList();
            int dem = unitkillScores.Count;
            foreach (var item in unitkillScores)
            {
                percent += item.TotalCount == 0 ? 0 : (item.CorrectCount / item.TotalCount) * (percentSkill / dem);
            }
            return Task.FromResult(percent);
        }

        #endregion Tinh Diem Unit

        #region Update Unit

        public async Task UpdateUnit(IList<LessonResult>? lessonResults, Domain.Entities.Unit? unit, Guid courseId, Guid studentId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonResults);
            ArgumentNullException.ThrowIfNull(unit);
            var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == unit.Id && x.StudentId == studentId && x.CourseId == courseId, cancellationToken);
            if (unit != null && unitResult != null)
            {
                var (groupedSkillScores, percent) = await UpdateUnitResult(lessonResults);
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

        #region Update Done Course

        public async Task UpdateCourseResult(Guid courseId, Guid studentId, CancellationToken cancellationToken)
        {
            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == courseId && x.StudentId == studentId, cancellationToken);
            if (courseResult != null)
            {
                await _finishOneLevelPassPublisher.Publish(courseResult, cancellationToken);
                courseResult.Status = EnumCourseStatus.InActive;
                _courseResultRepository.Update(courseResult);
                await _courseResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        #endregion Update Done Course
    }
}
