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

    public class ClassForumResultInputThenUpdateLessonResultHandler :
        INotificationHandler<EntityChangedEvent<ClassForumResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;

        public ClassForumResultInputThenUpdateLessonResultHandler(ILessonResultRepository lessonResultRepository)
        {
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task Handle(EntityChangedEvent<ClassForumResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.ClassForumResults).ThenInclude(x => x.ClassForumScores)
                                        .FirstOrDefaultAsync(x => x.Id == notification.Data.LessonResultId, cancellationToken);
            var status = notification.Data.Status;
            if (lessonResult != null && notification.Data.Status == EnumClassForumResultStatus.Graded)
            {
                lessonResult.Percent += ((double)notification.Data.ClassForumScores.Sum(x => x.Score) / 36) * 30 / 100;
                _lessonResultRepository.Update(lessonResult);
                await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
