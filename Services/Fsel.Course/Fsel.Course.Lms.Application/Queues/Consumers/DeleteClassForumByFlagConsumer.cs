// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Course.Lms.Application.Commands.ClassForumResultCmd;
    using Fsel.Shared.Models.ShareModels;
    using MassTransit;
    using MediatR;

    public class DeleteClassForumByFlagConsumer : IConsumer<FlagQueueModel>
    {
        private readonly IMediator _mediator;

        public DeleteClassForumByFlagConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<FlagQueueModel> context)
        {
            if (context == null)
            {
                return;
            }
            var data = context.Message;

            var classForum = new DeleteClassForumFlaggedCommand
            {
                Ids = data.ObjectIds,
                InteractionType = data.Type,
                Status = data.Status,
            };
            await _mediator.Send(classForum).ConfigureAwait(false);
        }
    }
}
