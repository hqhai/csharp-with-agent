// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Application.Commands.Events;
    using MassTransit;
    using MediatR;

    public class JobActiveEventConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public JobActiveEventConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public async Task ConsumeQueue(BaseQueueModel message)
        {
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new JobActiveEventCommand()
            ).ConfigureAwait(false);
        }
    }
}
