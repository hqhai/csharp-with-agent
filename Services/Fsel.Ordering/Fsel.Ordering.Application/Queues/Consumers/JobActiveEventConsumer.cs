// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class JobActiveEventConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public JobActiveEventConsumer(IMediator mediator, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(BaseQueueModel? message)
        {
            if (message == null)
            {
                return;
            }
            //await _mediator.Send(new JobActiveEventCommand()).ConfigureAwait(false);
        }
    }
}
