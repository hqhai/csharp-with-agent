// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queues.Consumers
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using MassTransit;
    using MediatR;

    public class DeleteGuestStudentConsumer : IConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public DeleteGuestStudentConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<BaseQueueModel> context)
        {
            if (context == null)
            {
                return;
            }
            var data = context.Message;

            var classForum = new DeleteGuestStudentByIdCommand
            {
                Id = Guid.Parse(data.QueueId!),
            };
            await _mediator.Send(classForum).ConfigureAwait(false);
        }
    }
}
