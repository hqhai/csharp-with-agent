// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Course.Lms.Application.Commands.ClassForumResultCmd;
    using Fsel.Shared.Models.ShareModels;
    using MassTransit;
    using MediatR;

    public class DeleteClassForumByFlagConsumer : Core.Base.Interfaces.IBaseConsumer<DeleteClassForumByFlagQueueModel>
    {
        private readonly IMediator _mediator;

        public DeleteClassForumByFlagConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<Core.Base.BaseModels.BaseQueueDataModel<DeleteClassForumByFlagQueueModel>> context)
        {
            if (context == null)
            {
                return;
            }
            var data = context.Message?.Data;

            var classForum = new DeleteClassForumByFlagCommand
            {
                Ids = data.ObjectIds,

            };
            await _mediator.Send(classForum).ConfigureAwait(false);
        }
    }
}
