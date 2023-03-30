// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using MediatR;

    public class CreateAnswerThenUpdateResultHandler : INotificationHandler<EntityCreatedEvent<VideoTimeCodeAnswer>>
    {

        public CreateAnswerThenUpdateResultHandler()
        {
        }

        public async Task Handle(EntityCreatedEvent<VideoTimeCodeAnswer> notification, CancellationToken cancellationToken)
        {

        }
    }
}
