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
            var classFormResult = notification.Data;
            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.VideoResult).Include(x => x.HomeWorkResults).Include(x => x.ClassForumResults).ThenInclude(x => x.ClassForumScores)
                                        .FirstOrDefaultAsync(x => x.Id == classFormResult.LessonResultId, cancellationToken);
            if (lessonResult != null && lessonResult.VideoResult != null && lessonResult.HomeWorkResults != null && classFormResult.Status == EnumClassForumResultStatus.Graded)
            {
                if (lessonResult.VideoResult.Status == EnumResultStatus.Done)
                {
                    var isCheckHomeWork = lessonResult.HomeWorkResults.All(x => x.Status == EnumResultStatus.Done);
                    var percentClassForum = ((double)classFormResult.ClassForumScores.Sum(x => x.Score) / 36) * 30;
                    var percentHomeWork = isCheckHomeWork ? (double)lessonResult.HomeWorkResults.Average(x => x.Percent) * 30 : 0;
                    var percentVideo = lessonResult.VideoResult.Percent * 40;
                    var percent = (percentClassForum + percentHomeWork + percentVideo) / 100;
                    lessonResult.Percent = percent;
                    _lessonResultRepository.Update(lessonResult);
                    await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
