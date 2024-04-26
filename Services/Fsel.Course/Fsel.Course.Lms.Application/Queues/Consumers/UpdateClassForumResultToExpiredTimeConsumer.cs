// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Commands.ClassForumResultCmd;
    using MassTransit;
    using MediatR;

    public class UpdateClassForumResultToExpiredTimeConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public UpdateClassForumResultToExpiredTimeConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(BaseQueueModel? message)
        {
            var queueId = message?.QueueId;
            if (queueId == null)
            {
                return;
            }
            await _mediator.Send(new UpdateClassForumResultToExpiredTimeCommand { ClassForumResultId = new Guid(queueId) }).ConfigureAwait(false);
        }
    }
}
