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

    public class VideoResultInputThenUpdateLessionResultHandler :
        INotificationHandler<EntityChangedEvent<VideoResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;

        public VideoResultInputThenUpdateLessionResultHandler(ILessonResultRepository lessonResultRepository)
        {
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task Handle(EntityChangedEvent<VideoResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var videoResult = notification.Data;
            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.VideoResult).Include(x => x.HomeWorkResults).Include(x => x.ClassForumResults).ThenInclude(x => x.ClassForumScores)
                                         .FirstOrDefaultAsync(x => x.Id == videoResult.LessonResultId, cancellationToken);
            if (lessonResult != null && lessonResult.VideoResult != null && lessonResult.HomeWorkResults != null && lessonResult.ClassForumResults != null)
            {
                if (videoResult.Status == EnumResultStatus.Done)
                {
                    var classForumResult = lessonResult.ClassForumResults.FirstOrDefault();
                    var isCheckHomeWork = lessonResult.HomeWorkResults.All(x => x.Status == EnumResultStatus.Done) && classForumResult != null;
                    var isCheckClassForum = classForumResult?.Status == EnumClassForumResultStatus.Graded;
                    var percentHomeWork = isCheckHomeWork ? (double)lessonResult.HomeWorkResults.Average(x => x.Percent) * 30 : 0;
                    var percentClassForum = isCheckClassForum ? ((double)classForumResult!.ClassForumScores.Sum(x => x.Score) / 36) * 30 : 0;
                    var percentVideo = lessonResult.VideoResult.Percent * 40;
                    var percent = (percentClassForum + percentHomeWork + percentVideo) / 100;
                    lessonResult.Percent = percent;
                    lessonResult.Status = EnumResultStatus.Done;
                    _lessonResultRepository.Update(lessonResult);
                    await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
