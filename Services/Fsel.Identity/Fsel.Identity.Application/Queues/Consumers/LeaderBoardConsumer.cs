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

        public LeaderBoardConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(BaseQueueModel? message)
        {
            await _mediator.Send(new CreateStudentRankingsCommand()).ConfigureAwait(false);
        }
    }
}
