// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class VideoResultInputThenUpdateLessonResultHandler :
        INotificationHandler<EntityChangedEvent<VideoResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly FinishOneLessonPublisher _finishOneLessonPublisher;
        private readonly VideoConverter _videoConverter;

        public VideoResultInputThenUpdateLessonResultHandler(ILessonResultRepository lessonResultRepository
            , FinishOneLessonPublisher finishOneLessonPublisher
            , VideoConverter videoConverter)

        {
            _lessonResultRepository = lessonResultRepository;
            _finishOneLessonPublisher = finishOneLessonPublisher;
            _videoConverter = videoConverter;
        }

        public async Task Handle(EntityChangedEvent<VideoResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var videoResult = notification.Data;
            var lessonResult = await _lessonResultRepository.Queryable
                                        .Include(x => x.VideoResult)
                                        .FirstOrDefaultAsync(x => x.Id == videoResult.LessonResultId, cancellationToken);
            if (lessonResult != null && lessonResult.VideoResult != null)
            {
                var skillScores = videoResult.VideoSkillScores?.FirstOrDefault(x => x.Type == EnumTimeCodeType.Standalone)?.SkillScores;
                lessonResult.CorrectCount = videoResult.CorrectCount;
                lessonResult.Percent = videoResult.Percent * 40 / 100;
                lessonResult.SkillScores = skillScores;
                if (videoResult.Status == EnumResultStatus.Done)
                {
                    await _finishOneLessonPublisher.Publish(lessonResult, cancellationToken);
                    lessonResult.Status = EnumResultStatus.Done;
                }
                _lessonResultRepository.Update(lessonResult);
                await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
