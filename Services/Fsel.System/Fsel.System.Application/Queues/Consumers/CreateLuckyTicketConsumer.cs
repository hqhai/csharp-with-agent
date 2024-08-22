// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.LuckyTickets;
    using MediatR;

    public class CreateLuckyTicketConsumer : BaseConsumer<LuckyTicketQueueModel>
    {
        private readonly IMediator _mediator;

        public CreateLuckyTicketConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(LuckyTicketQueueModel? message)
        {
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new CreateLuckyTicketCommand
            {
                LessonResultId = message.LessonResultId
            }).ConfigureAwait(false);
        }
    }
}
