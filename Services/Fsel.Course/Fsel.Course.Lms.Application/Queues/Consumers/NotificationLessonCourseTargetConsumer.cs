// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Commands.LessonCmd;
    using MediatR;

    public class NotificationLessonCourseTargetConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public NotificationLessonCourseTargetConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(BaseQueueModel? message)
        {
            await _mediator.Send(new SendWeeklyLessonCourseTargetCommand()).ConfigureAwait(false);
        }
    }
}
