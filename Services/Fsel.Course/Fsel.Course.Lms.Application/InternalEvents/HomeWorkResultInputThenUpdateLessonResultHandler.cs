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

    public class HomeWorkResultInputThenUpdateLessonResultHandler :
        INotificationHandler<EntityChangedEvent<HomeWorkResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;

        public HomeWorkResultInputThenUpdateLessonResultHandler(ILessonResultRepository lessonResultRepository)
        {
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task Handle(EntityChangedEvent<HomeWorkResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.HomeWorkResults).FirstOrDefaultAsync(x => x.Id == notification.Data.LessonResultId, cancellationToken);
            if (lessonResult != null && lessonResult.HomeWorkResults.All(x => x.Status == EnumResultStatus.Done))
            {
                lessonResult.Percent += (notification.Data.CorrectCount / notification.Data.CorrectTotal) * 30 / 100;
                _lessonResultRepository.Update(lessonResult);
                await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
