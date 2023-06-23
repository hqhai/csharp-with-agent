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

    public class LessonResultInputThenUpdateUnitResultHandler :
        INotificationHandler<EntityChangedEvent<LessonResult>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IUnitResultRepository _unitResultRepository;

        public LessonResultInputThenUpdateUnitResultHandler(IUnitRepository unitRepository, IUnitResultRepository unitResultRepository)
        {
            _unitRepository = unitRepository;
            _unitResultRepository = unitResultRepository;
        }

        public async Task Handle(EntityChangedEvent<LessonResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var unit = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == notification.Data.StudentId && x.CourseId == notification.Data.CourseId))
                                                    .Include(x => x.UnitLessons)
                                                    .FirstOrDefaultAsync(x => x.Id == notification.Data.UnitId, cancellationToken);
            if (unit != null)
            {
                var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == notification.Data.UnitId && x.StudentId == notification.Data.StudentId && x.CourseId == notification.Data.CourseId, cancellationToken);
                if (unitResult != null && unit.LessonResults.Count == unit.UnitLessons.Count)
                {
                    unitResult.Status = EnumResultStatus.Done;
                    unitResult.Percent += notification.Data.Percent * 18 / 100;
                    _unitResultRepository.Update(unitResult);
                    await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
