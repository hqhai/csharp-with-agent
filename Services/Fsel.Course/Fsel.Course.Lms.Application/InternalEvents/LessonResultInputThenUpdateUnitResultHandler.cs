// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class LessonResultInputThenUpdateUnitResultHandler : BaseInternalEventHandler,
        INotificationHandler<EntityChangedEvent<LessonResult>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public LessonResultInputThenUpdateUnitResultHandler(IUnitRepository unitRepository
            , IUnitResultRepository unitResultRepository
            , ILessonResultRepository lessonResultRepository
            , IVideoResultRepository videoResultRepository
            , IClassForumResultRepository classForumResultRepository
            , IHomeWorkResultRepository homeWorkResultRepository
            ) : base(videoResultRepository, classForumResultRepository, homeWorkResultRepository)
        {
            _unitRepository = unitRepository;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task Handle(EntityChangedEvent<LessonResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var unit = await _unitRepository.Queryable.Include(x => x.LessonResults)
                                                    .Include(x => x.UnitLessons)
                                                    .Include(x => x.UnitSkillMockTests)
                                                    .Where(x => x.LessonResults.Any(x => x.Status == EnumResultStatus.Done && x.StudentId == notification.Data.StudentId && x.CourseId == notification.Data.CourseId))
                                                    .FirstOrDefaultAsync(x => x.Id == notification.Data.UnitId, cancellationToken);
            if (unit != null)
            {
                var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == notification.Data.UnitId && x.StudentId == notification.Data.StudentId && x.CourseId == notification.Data.CourseId, cancellationToken);
                if (unitResult != null && unit.LessonResults.Count == unit.UnitLessons.Count && unit.UnitSkillMockTests.Count == 0)
                {
                    List<SkillScores> videoSkillScores = new List<SkillScores>();
                    List<SkillScores> homeSkillScores = new List<SkillScores>();
                    List<SkillScores> classForumSkillScores = new List<SkillScores>();
                    List<SkillScores> skillTestSkillScores = new List<SkillScores>();
                    List<SkillScores> unitTestSkillScores = new List<SkillScores>();
                    foreach (var item in unit.LessonResults)
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
                    await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
