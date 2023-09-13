// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class VideoResultInputThenUpdateLessonResultHandler :
        INotificationHandler<EntityChangedEvent<VideoResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly FinishOneLessonPublisher _finishOneLessonPublisher;

        public VideoResultInputThenUpdateLessonResultHandler(ILessonResultRepository lessonResultRepository
            , FinishOneLessonPublisher finishOneLessonPublisher)

        {
            _lessonResultRepository = lessonResultRepository;
            _finishOneLessonPublisher = finishOneLessonPublisher;
        }

        public async Task Handle(EntityChangedEvent<VideoResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var videoResult = notification.Data;
            var lessonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == videoResult.LessonResultId, cancellationToken);
            if (lessonResult != null && videoResult.Status == EnumResultStatus.Done)
            {
                var skillScores = videoResult.VideoSkillScores?.FirstOrDefault(x => x.Type == EnumTimeCodeType.Standalone)?.SkillScores;
                lessonResult.CorrectCount = videoResult.CorrectCount;
                lessonResult.CorrectTotal = videoResult.CorrectTotal;
                lessonResult.Percent = NumberHelper.ConvertDoublePercent(videoResult.Percent * 40);
                lessonResult.SkillScores = skillScores;
                await _finishOneLessonPublisher.Publish(lessonResult, cancellationToken);
                _lessonResultRepository.Update(lessonResult);
                await _lessonResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
