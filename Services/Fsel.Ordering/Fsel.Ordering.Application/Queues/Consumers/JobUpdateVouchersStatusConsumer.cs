// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Application.Commands.VoucherCmds;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class JobUpdateVouchersStatusConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public JobUpdateVouchersStatusConsumer(IMediator mediator, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(BaseQueueModel? message)
        {
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new JobUpdateVouchersStatusCommand()
            ).ConfigureAwait(false);
        }
    }
}
