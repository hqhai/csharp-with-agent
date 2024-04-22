// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Commands.ClassForumResultCmd;
    using MassTransit;
    using MediatR;

    public class UpdateClassForumResultToExpiredTimeConsumer : IConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public UpdateClassForumResultToExpiredTimeConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<BaseQueueModel> context)
        {
            if (context == null || context.Message == null || context.Message.QueueId == null)
            {
                return;
            }
            await _mediator.Send(new UpdateClassForumResultToExpiredTimeCommand { ClassForumResultId = new Guid(context.Message.QueueId) }).ConfigureAwait(false);
        }
    }
}
