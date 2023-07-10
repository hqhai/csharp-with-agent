// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public class BaseInternalEventHandler
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;

        public BaseInternalEventHandler(IVideoResultRepository videoResultRepository,
            IClassForumResultRepository classForumResultRepository,
            IUnitResultRepository unitResultRepository,
            ILessonResultRepository lessonResultRepository,
            ICourseRepository courseRepository,
            IFinalTestResultRepository finalTestResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository)
        {
            _videoResultRepository = videoResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseRepository = courseRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
        }

        #region Get Skill Scores

        public async Task<List<SkillScores>> VideoSkillScores(Guid? lessonResultid)
        {
            ArgumentNullException.ThrowIfNull(lessonResultid);
            List<SkillScores> skillScores = new List<SkillScores>();
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.Status == EnumResultStatus.Done && x.LessonResultId == lessonResultid);
            if (videoResult != null && videoResult.VideoSkillScores != null)
            {
                skillScores = videoResult.VideoSkillScores.Where(x => x.Type == EnumTimeCodeType.Standalone).SelectMany(x => x.SkillScores!).Where(x => x.TotalCount != 0).ToList();
            }
            return skillScores;
        }

        public async Task<List<SkillScores>> UnitTestSkillScores(Guid? lessonResultid)
        {
            ArgumentNullException.ThrowIfNull(lessonResultid);
            List<SkillScores> skillScores = new List<SkillScores>();
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.Status == EnumResultStatus.Done && x.LessonResultId == lessonResultid);
            if (videoResult != null && videoResult.VideoSkillScores != null)
            {
                skillScores = videoResult.VideoSkillScores.Where(x => x.Type == EnumTimeCodeType.UnitTest).SelectMany(x => x.SkillScores!).Where(x => x.TotalCount != 0).ToList();
            }
            return skillScores;
        }

        public async Task<List<SkillScores>> SkillTestSkillScores(Guid? lessonResultid)
        {
            ArgumentNullException.ThrowIfNull(lessonResultid);
            List<SkillScores> skillScores = new List<SkillScores>();
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.Status == EnumResultStatus.Done && x.LessonResultId == lessonResultid);
            if (videoResult != null && videoResult.VideoSkillScores != null)
            {
                skillScores = videoResult.VideoSkillScores.Where(x => x.Type == EnumTimeCodeType.SkillTest).SelectMany(x => x.SkillScores!).Where(x => x.TotalCount != 0).ToList();
            }
            return skillScores;
        }

        public async Task<SkillScores> ClassForumSkillScores(Guid? lessonResultid)
        {
            ArgumentNullException.ThrowIfNull(lessonResultid);
            SkillScores skillScores = new SkillScores();
            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForum).Include(x => x.ClassForumScores).FirstOrDefaultAsync(x => x.Status == EnumClassForumResultStatus.Graded && x.LessonResultId == lessonResultid);
            if (classForumResult != null && classForumResult.ClassForumScores != null && classForumResult.ClassForum != null)
            {
                skillScores.Skill = classForumResult.ClassForum.CourseSkill;
                skillScores.TotalCount = 36;
                skillScores.CorrectCount = classForumResult.ClassForumScores.Sum(x => x.Score);
            }
            return skillScores;
        }

        public async Task<List<SkillScores>> HomeWordsSkillScores(Guid? lessonResultid)
        {
            ArgumentNullException.ThrowIfNull(lessonResultid);
            List<SkillScores> skillScores = new List<SkillScores>();
            var homeWorkResults = await _homeWorkResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.LessonResultId == lessonResultid).ToArrayAsync();
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

        #endregion Get Skill Scores

        #region Tinh Diem Unit

        public async Task<double> PercentUnit(IList<SkillScores>? skillScores, int percentSkill)
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
            return percent;
        }

        #endregion Tinh Diem Unit

        #region Update Unit

        public async Task UpdateUnit(IList<LessonResult>? lessonResults, Domain.Entities.Unit? unit, Guid courseId, Guid studentId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonResults);
            ArgumentNullException.ThrowIfNull(unit);
            var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == unit.Id && x.StudentId == studentId && x.CourseId == courseId, cancellationToken);
            if (unit != null && unitResult != null && unit.LessonResults.Count == unit.UnitLessons.Count && unit.UnitSkillMockTests.Count == 0)
            {
                List<SkillScores> videoSkillScores = new List<SkillScores>();
                List<SkillScores> homeSkillScores = new List<SkillScores>();
                List<SkillScores> classForumSkillScores = new List<SkillScores>();
                List<SkillScores> skillTestSkillScores = new List<SkillScores>();
                List<SkillScores> unitTestSkillScores = new List<SkillScores>();
                foreach (var item in lessonResults)
                {
                    var lesssonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == item.Id, cancellationToken);
                    if (lesssonResult != null)
                    {
                        videoSkillScores.AddRange(await VideoSkillScores(lesssonResult.Id));
                        unitTestSkillScores.AddRange(await UnitTestSkillScores(lesssonResult.Id));
                        skillTestSkillScores.AddRange(await SkillTestSkillScores(lesssonResult.Id));
                        homeSkillScores.AddRange(await HomeWordsSkillScores(lesssonResult.Id));
                        classForumSkillScores.Add(await ClassForumSkillScores(lesssonResult.Id));
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
                                        Scores = group.Sum(x => x.Scores),
                                        TotalCount = group.Sum(x => x.TotalCount),
                                        CorrectCount = group.Sum(x => x.CorrectCount)
                                    })
                                    .ToList();

                unitResult.CorrectCount = (int)groupedSkillScores.Sum(x => x.CorrectCount);
                unitResult.CorrectTotal = (int)groupedSkillScores.Sum(x => x.TotalCount);
                unitResult.Status = EnumResultStatus.Done;
                unitResult.Percent = await PercentUnit(videoSkillScores, 18) + await PercentUnit(homeSkillScores, 22) + await PercentUnit(classForumSkillScores, 20) + await PercentUnit(skillTestSkillScores, 10) + await PercentUnit(unitTestSkillScores, 30);
                unitResult.SkillScores = groupedSkillScores;
                _unitResultRepository.Update(unitResult);
                await _unitResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        #endregion Update Unit

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
                    var isCheckDone = false;
                    var isCheckUnitResults = course.UnitResults.Where(x => x.StudentId == unitResult.StudentId).All(x => x.Status == EnumResultStatus.Done);
                    switch (course.CourseLevel.GetEnumCourseType())
                    {
                        case EnumCourseType.Ielts:
                            isCheckDone = isCheckUnitResults && course.MockTestResults.Where(x => x.StudentId == unitResult.StudentId).All(x => x.Status == EnumResultStatus.Done);
                            break;

                        case EnumCourseType.Academic:
                            isCheckDone = isCheckUnitResults && course.FinalTestResults.Where(x => x.StudentId == unitResult.StudentId).All(x => x.Status == EnumResultStatus.Done);
                            break;

                        default:
                            break;
                    }
                    var displayOrder = course.CourseUnitMockTests.FirstOrDefault(x => x.UnitId == unitResult.UnitId)?.DisplayOrder;
                    var courseUnitMockTest = course.CourseUnitMockTests.FirstOrDefault(x => x.DisplayOrder == displayOrder + 1);
                    if (!isCheckDone && courseUnitMockTest != null)
                    {
                        await UpdateStatusProcess(courseUnitMockTest, unitResult.StudentId, cancellationToken);
                    }
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
                    var isCheckDone = false;
                    var isCheckUnitResults = course.UnitResults.Where(x => x.StudentId == mockTestResult.StudentId).All(x => x.Status == EnumResultStatus.Done);
                    switch (course.CourseLevel.GetEnumCourseType())
                    {
                        case EnumCourseType.Ielts:
                            isCheckDone = isCheckUnitResults && course.MockTestResults.Where(x => x.StudentId == mockTestResult.StudentId).All(x => x.Status == EnumResultStatus.Done);
                            break;

                        case EnumCourseType.Academic:
                            isCheckDone = isCheckUnitResults && course.FinalTestResults.Where(x => x.StudentId == mockTestResult.StudentId).All(x => x.Status == EnumResultStatus.Done);
                            break;

                        default:
                            break;
                    }
                    var displayOrder = course.CourseUnitMockTests.FirstOrDefault(x => x.MockTestId == mockTestResult.MockTestId)?.DisplayOrder;
                    var courseUnitMockTest = course.CourseUnitMockTests.FirstOrDefault(x => x.DisplayOrder == displayOrder + 1);
                    if (!isCheckDone && courseUnitMockTest != null)
                    {
                        await UpdateStatusProcess(courseUnitMockTest, mockTestResult.StudentId, cancellationToken);
                    }
                }
            }
        }

        public async Task UpdateProcessFinalTest(FinalTestResult? finalTestResult, CancellationToken cancellationToken)
        {
            var courseId = finalTestResult != null ? finalTestResult.CourseId : default;
            if (courseId != default && finalTestResult != null)
            {
                var course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests)
                                                  .Include(x => x.UnitResults)
                                                  .Include(x => x.MockTestResults)
                                                  .Include(x => x.FinalTestResults)
                                                  .FirstOrDefaultAsync(x => x.Id == courseId, cancellationToken);
                if (course != null)
                {
                    var isCheckDone = false;
                    var isCheckUnitResults = course.UnitResults.Where(x => x.StudentId == finalTestResult.StudentId).All(x => x.Status == EnumResultStatus.Done);
                    switch (course.CourseLevel.GetEnumCourseType())
                    {
                        case EnumCourseType.Ielts:
                            isCheckDone = isCheckUnitResults && course.MockTestResults.Where(x => x.StudentId == finalTestResult.StudentId).All(x => x.Status == EnumResultStatus.Done);
                            break;

                        case EnumCourseType.Academic:
                            isCheckDone = isCheckUnitResults && course.FinalTestResults.Where(x => x.StudentId == finalTestResult.StudentId).All(x => x.Status == EnumResultStatus.Done);
                            break;

                        default:
                            break;
                    }
                    var displayOrder = course.CourseUnitMockTests.FirstOrDefault(x => x.FinalTestId == finalTestResult.FinalTestId)?.DisplayOrder;
                    var courseUnitMockTest = course.CourseUnitMockTests.FirstOrDefault(x => x.DisplayOrder == displayOrder + 1);
                    if (!isCheckDone && courseUnitMockTest != null)
                    {
                        await UpdateStatusProcess(courseUnitMockTest, finalTestResult.StudentId, cancellationToken);
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
                        unitResultNext.Status = EnumResultStatus.Process;
                        _unitResultRepository.Update(unitResultNext);
                        await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    break;

                case var value when value == (courseUnitMockTest.FinalTestId == null):
                    var finalTestResultNext = await _finalTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.Id == courseUnitMockTest.FinalTestId, cancellationToken);
                    if (finalTestResultNext != null)
                    {
                        finalTestResultNext.Status = EnumResultStatus.Process;
                        _finalTestResultRepository.Update(finalTestResultNext);
                        await _finalTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    break;

                case var value when value == (courseUnitMockTest.MockTestId == null):
                    var mockTestResultNext = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.Id == courseUnitMockTest.MockTestId, cancellationToken);
                    if (mockTestResultNext != null)
                    {
                        mockTestResultNext.Status = EnumResultStatus.Process;
                        _mockTestResultRepository.Update(mockTestResultNext);
                        await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
