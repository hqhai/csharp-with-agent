// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Identity.Application.Commands.StudentRankingCmd;
using MassTransit;
using MediatR;

namespace Fsel.Identity.Application.Queues.Consumers
{
    public class LeaderBoardConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public LeaderBoardConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(ConsumeContext<BaseQueueDataModel<BaseQueueModel>> context)
        {
            await _mediator.Send(new CreateStudentRankingsCommand()).ConfigureAwait(false);
        }
    }
}
