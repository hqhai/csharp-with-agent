// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public class BaseInternalLessonResultEventHandler : BaseInternalEventHandler
    {
        public BaseInternalLessonResultEventHandler(IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ILessonResultRepository lessonResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IMockTestRepository mockTestRepository, IHomeWorkQuestionRepository homeWorkQuestionRepository, IHomeWorkAnswerRepository homeWorkAnswerRepository, IQuestionRepository questionRepository, IHomeWorkRepository homeWorkRepository, FinishOneUnitPublisher finishOneUnitPublisher, FinishOneLevelPassPublisher finishOneLevelPassPublisher, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository) : base(videoResultRepository, classForumResultRepository, unitResultRepository, lessonResultRepository, courseResultRepository, courseRepository, unitRepository, mockTestRepository, homeWorkQuestionRepository, homeWorkAnswerRepository, questionRepository, homeWorkRepository, finishOneUnitPublisher, finishOneLevelPassPublisher, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository)
        {
        }

        public async Task GetLessonResult(LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonResult);
            var (correctVideo, totalVideo, percentVideo, skillScoreVideos) = await GetVideoResult(lessonResult.Id, cancellationToken);
            var (correctHomeWork, totalHomeWork, percentHomeWork, skillScoreHomeWorks) = await GetHomeResults(lessonResult.Id, cancellationToken);
            var (correctClassForum, totalClassForum, percentClassForum, skillScoreClassForums) = await GetClassForumResult(lessonResult.Id, cancellationToken);

            var skillScores = new List<SkillScores>();
            if (skillScoreVideos != null && skillScoreVideos.Any())
            {
                skillScores = skillScores.Union(skillScoreVideos).ToList();
            }
            if (skillScoreHomeWorks != null && skillScoreHomeWorks.Any())
            {
                skillScores = skillScores.Union(skillScoreHomeWorks).ToList();
            }
            if (skillScoreClassForums != null && skillScoreClassForums.Any())
            {
                skillScores = skillScores.Union(skillScoreClassForums).ToList();
            }
            List<SkillScores> groupedSkillScores = skillScores
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
            lessonResult.CorrectCount = (int)(correctVideo + correctClassForum + correctHomeWork ?? default);
            lessonResult.CorrectTotal = (int)(totalVideo + totalHomeWork + totalClassForum ?? default);
            lessonResult.Percent = percentVideo * 40 + percentHomeWork * 30 + percentClassForum * 40 ?? default;
            lessonResult.SkillScores = groupedSkillScores;
        }

        public async Task<(double?, double?, double?, IList<SkillScores>?)> GetVideoResult(Guid lessonResultId, CancellationToken cancellationToken)
        {
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == lessonResultId, cancellationToken);
            if (videoResult != null)
            {
                var skillScores = videoResult.VideoSkillScores?.FirstOrDefault(x => x.Type == EnumTimeCodeType.Standalone)?.SkillScores;
                if (skillScores != null && skillScores.Count > 0)
                {
                    return (skillScores.Sum(x => x.TotalCount), skillScores.Sum(x => x.TotalCount), videoResult.Percent, skillScores);
                }
            }

            return (null, null, default, null);
        }

        public async Task<(double?, double?, double, IList<SkillScores>?)> GetClassForumResult(Guid lessonResultId, CancellationToken cancellationToken)
        {
            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForumScores)
                                                                            .Include(x => x.ClassForum)
                                                                            .FirstOrDefaultAsync(x => x.LessonResultId == lessonResultId, cancellationToken);
            if (classForumResult != null)
            {
                var skillScores = new SkillScores
                {
                    CorrectCount = classForumResult.ClassForumScores?.Sum(x => x.Score) ?? default,
                    CountQuestion = 1,
                    TotalQuestion = 1,
                    TotalCount = 36,
                    Skill = classForumResult.ClassForum?.CourseSkill ?? default,
                };
                skillScores.Percent = (double)skillScores.CorrectCount / skillScores.TotalCount;
                return (skillScores.CorrectCount, skillScores.TotalCount, skillScores.Percent, new List<SkillScores> { skillScores });
            }

            return (null, null, default, null);
        }

        public async Task<(double?, double?, double, IList<SkillScores>?)> GetHomeResults(Guid lessonResultId, CancellationToken cancellationToken)
        {
            var skillScoreQuery = from baseQ in _lessonResultRepository.Queryable
                                  join hr in _homeWorkResultRepository.Queryable on baseQ.Id equals hr.LessonResultId
                                  join h in _homeWorkRepository.Queryable on hr.HomeWorkId equals h.Id
                                  join hq in _homeWorkQuestionRepository.Queryable on h.Id equals hq.HomeWorkId
                                  join q in _questionRepository.Queryable on hq.QuestionId equals q.Id
                                  join ha in _homeWorkAnswerRepository.Queryable on hr.Id equals ha.HomeWorkResultId
                                  where baseQ.Id == lessonResultId
                                  group new { h, ha, q } by h.CourseSkill into g
                                  select new SkillScores
                                  {
                                      Skill = g.Key,
                                      CorrectCount = g.Select(x => x.ha).Sum(x => x.CorrectCount),
                                      TotalCount = g.Select(x => x.q).Sum(x => x.CorrectTotal),
                                      CountQuestion = g.Select(x => x.q).Count(),
                                      TotalQuestion = g.Select(x => x.ha).Count(),
                                  };
            if (skillScoreQuery.Any())
            {
                var skillScores = await skillScoreQuery.ToListAsync(cancellationToken);
                skillScores.ForEach(x => x.Percent = x.TotalCount > 0 ? NumberHelper.ConvertDouble(x.CorrectCount / x.TotalCount) : default);
                return (skillScores.Sum(x => x.TotalCount), skillScores.Sum(x => x.TotalCount), skillScores.Average(x => x.Scores), skillScores);
            }
            return (null, null, default, null);
        }

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
    }
}
