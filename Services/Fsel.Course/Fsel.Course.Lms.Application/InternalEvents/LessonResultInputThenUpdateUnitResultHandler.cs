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

    public class LessonResultInputThenUpdateUnitResultHandler :
        INotificationHandler<EntityChangedEvent<LessonResult>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public LessonResultInputThenUpdateUnitResultHandler(IUnitRepository unitRepository, IUnitResultRepository unitResultRepository, ILessonResultRepository lessonResultRepository)
        {
            _unitRepository = unitRepository;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task Handle(EntityChangedEvent<LessonResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var unit = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == notification.Data.StudentId && x.CourseId == notification.Data.CourseId))
                                                    .Include(x => x.UnitLessons)
                                                    .Include(x => x.UnitSkillMockTests)
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
                        var lesssonResult = await _lessonResultRepository.Queryable.Include(x => x.VideoResult)
                                                                             .Include(x => x.HomeWorkResults)
                                                                             .Include(x => x.ClassForumResults)
                                                                             .ThenInclude(x => x.ClassForum)
                                                                             .Include(x => x.ClassForumResults)
                                                                             .ThenInclude(x => x.ClassForumScores)
                                                                             .FirstOrDefaultAsync(x => x.Id == item.Id, cancellationToken);
                        if (lesssonResult != null)
                        {
                            videoSkillScores.AddRange(await VideoSkillScores(lesssonResult.VideoResult));
                            unitTestSkillScores.AddRange(lesssonResult.VideoResult.VideoSkillScores!.Where(x => x.Type == EnumTimeCodeType.SkillTest).SelectMany(x => x.SkillScores!).ToList());
                            skillTestSkillScores.AddRange(lesssonResult.VideoResult.VideoSkillScores!.Where(x => x.Type == EnumTimeCodeType.UnitTest).SelectMany(x => x.SkillScores!).ToList());
                            homeSkillScores.AddRange(lesssonResult.HomeWorkResults.SelectMany(x => x.SkillScores!).ToList());
                        }
                    }

                    unitResult.Status = EnumResultStatus.Done;
                    unitResult.Percent += notification.Data.Percent * 18 / 100;
                    _unitResultRepository.Update(unitResult);
                    await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task<List<SkillScores>> VideoSkillScores(VideoResult? videoResult)
        {
            ArgumentNullException.ThrowIfNull(videoResult);
            List<SkillScores> skillScores = new List<SkillScores>();
            skillScores = videoResult.VideoSkillScores!.Where(x => x.Type == EnumTimeCodeType.Standalone).SelectMany(x => x.SkillScores!).ToList();
            skillScores = skillScores.Where(x => x.TotalCount == 0 && x.CorrectCount == 0).ToList();
            return skillScores;
        }
    }
}
