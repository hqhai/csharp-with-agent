// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queues.Consumers
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using MassTransit;
    using MediatR;

    public class DeleteGuestStudentConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public DeleteGuestStudentConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(ConsumeContext<BaseQueueDataModel<BaseQueueModel>> context)
        {
            if (context == null)
            {
                return;
            }
            var data = context.Message;

            var classForum = new DeleteGuestStudentByUserIdCommand
            {
                Id = Guid.Parse(data.Data.QueueId!),
            };
            await _mediator.Send(classForum).ConfigureAwait(false);
        }
    }
}
