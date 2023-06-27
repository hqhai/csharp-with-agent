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

    public class VideoResultInputThenUpdateLessionResultHandler :
        INotificationHandler<EntityChangedEvent<VideoResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IMediator _mediator;

        public VideoResultInputThenUpdateLessionResultHandler(ILessonResultRepository lessonResultRepository, IMediator mediator)
        {
            _lessonResultRepository = lessonResultRepository;
            _mediator = mediator;
        }

        public async Task Handle(EntityChangedEvent<VideoResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var lessonResult = await _lessonResultRepository.GetByIdAsync(notification.Data.LessonResultId);
            var status = notification.Data.Status;
            if (lessonResult != null && status == EnumResultStatus.Done)
            {
                lessonResult.Status = EnumResultStatus.Done;
                lessonResult.Percent += notification.Data.Percent * 40 / 100;
                _lessonResultRepository.Update(lessonResult);
                await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                await _mediator.Publish(new EntityChangedEvent<LessonResult>(lessonResult), cancellationToken);
            }
        }
    }
}
