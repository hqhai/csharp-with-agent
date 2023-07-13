// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class LessonResultInputThenUpdateUnitResultHandler : BaseInternalEventHandler,
        INotificationHandler<EntityChangedEvent<LessonResult>>
    {
        private readonly IUnitRepository _unitRepository;

        public LessonResultInputThenUpdateUnitResultHandler(IUnitRepository unitRepository
            , IUnitResultRepository unitResultRepository
            , ILessonResultRepository lessonResultRepository
            , IVideoResultRepository videoResultRepository
            , IClassForumResultRepository classForumResultRepository
            , IHomeWorkResultRepository homeWorkResultRepository
            , ICourseRepository courseRepository
            , ICourseResultRepository courseResultRepository
            , IMockTestResultRepository mockTestResultRepository
            , IFinalTestResultRepository finalTestResultRepository
            ) : base(videoResultRepository,
                classForumResultRepository,
                unitResultRepository,
                lessonResultRepository,
                courseResultRepository,
                courseRepository,
                finalTestResultRepository,
                mockTestResultRepository,
                homeWorkResultRepository)
        {
            _unitRepository = unitRepository;
        }

        public async Task Handle(EntityChangedEvent<LessonResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var lessonResult = notification.Data;
            var unit = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == lessonResult.StudentId && x.CourseId == lessonResult.CourseId))
                                                    .Include(x => x.UnitLessons)
                                                    .Include(x => x.UnitSkillMockTests)
                                                    .FirstOrDefaultAsync(x => x.Id == lessonResult.UnitId, cancellationToken);

            if (unit != null && lessonResult.Status == EnumResultStatus.Done)
            {
                await UpdateUnit(unit.LessonResults.ToList(), unit, lessonResult.CourseId, lessonResult.StudentId, cancellationToken);
            }
        }
    }
}
