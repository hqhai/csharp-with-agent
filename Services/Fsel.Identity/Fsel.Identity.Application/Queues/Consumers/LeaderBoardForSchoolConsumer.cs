// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Identity.Application.Commands.StudentCompetitionSnapShotCmd;
using MediatR;

namespace Fsel.Identity.Application.Queues.Consumers
{
    public class LeaderBoardForSchoolConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public LeaderBoardForSchoolConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(BaseQueueModel? message)
        {
            await _mediator.Send(new SaveSnapShotResultBySchoolCommand()).ConfigureAwait(false);
        }
    }
}
