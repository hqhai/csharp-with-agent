// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Identity.Application.Commands.StudentRankingCmd;
using MassTransit;
using MediatR;

namespace Fsel.Identity.Application.Queues.Consumers
{
    public class LeaderBoardConsumer : IConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public LeaderBoardConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<BaseQueueModel> context)
        {
            await _mediator.Send(new CreateStudentRankingsCommand()).ConfigureAwait(false);
        }
    }
}
