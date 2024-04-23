// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Commands.ClassForumResultCmd;
    using MassTransit;
    using MediatR;

    public class UpdateClassForumResultToExpiredTimeConsumer : Core.Base.Interfaces.IBaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public UpdateClassForumResultToExpiredTimeConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<BaseQueueDataModel<BaseQueueModel>> context)
        {
            var queueId = context?.Message.Data?.QueueId;
            if (context == null || queueId == null)
            {
                return;
            }
            await _mediator.Send(new UpdateClassForumResultToExpiredTimeCommand { ClassForumResultId = new Guid(queueId) }).ConfigureAwait(false);
        }
    }
}
