// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queues.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using Fsel.Shared.Models.ShareModels;
    using MassTransit;
    using MediatR;

    public class DeleteGuestStudentConsumer : IConsumer<DeleteGuestStudentQueueModel>
    {
        private readonly IMediator _mediator;

        public DeleteGuestStudentConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<DeleteGuestStudentQueueModel> context)
        {
            if (context == null)
            {
                return;
            }
            var data = context.Message;

            var classForum = new DeleteGuestStudentByIdCommand
            {
                Id = data.Id,

            };
            await _mediator.Send(classForum).ConfigureAwait(false);
        }
    }
}
