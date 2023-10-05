// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Commands.ClassForumResultCmd;
    using MassTransit;
    using MediatR;

    public class UpdateTeacherGradingInClassForumAndMockTestConsumer : IConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public UpdateTeacherGradingInClassForumAndMockTestConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<BaseQueueModel> context)
        {
            await _mediator.Send(new UpdateTeacherGradingInClassForumAndMockTestCommand()).ConfigureAwait(false);
        }
    }
}
