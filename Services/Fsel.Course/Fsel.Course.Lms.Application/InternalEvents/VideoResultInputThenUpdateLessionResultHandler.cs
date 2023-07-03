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
            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.VideoResult).Include(x => x.HomeWorkResults).Include(x => x.ClassForumResults).ThenInclude(x => x.ClassForumScores)
                                         .FirstOrDefaultAsync(x => x.Id == notification.Data.LessonResultId, cancellationToken);
            var status = notification.Data.Status;
            if (lessonResult != null && lessonResult.VideoResult != null && lessonResult.HomeWorkResults != null && status == EnumResultStatus.Done && lessonResult.ClassForumResults != null)
            {
                var isCheckHomeWork = lessonResult.HomeWorkResults.All(x => x.Status == EnumResultStatus.Done);
                var isCheckClassForum = lessonResult.ClassForumResults.FirstOrDefault()?.Status == EnumClassForumResultStatus.Graded;
                var classForumResult = lessonResult.ClassForumResults.FirstOrDefault();
                var percentHomeWork = isCheckHomeWork ? (double)lessonResult.HomeWorkResults.Sum(x => x.CorrectCount) / lessonResult.HomeWorkResults.Sum(x => x.CorrectTotal) * 30 : 0;
                var percentClassForum = isCheckClassForum ? ((double)lessonResult.ClassForumResults.SelectMany(x => x.ClassForumScores).Sum(x => x.Score) / 36) * 30 : 0;
                var isCheck = lessonResult.VideoResult.Status == EnumResultStatus.Done;
                if (isCheck)
                {
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
