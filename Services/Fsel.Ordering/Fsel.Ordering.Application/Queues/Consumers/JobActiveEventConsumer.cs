// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Application.Commands.Events;
    using MassTransit;
    using MediatR;

    public class JobActiveEventConsumer : IConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public JobActiveEventConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<BaseQueueModel> context)
        {
            var message = context?.Message;
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new JobActiveEventCommand()
            ).ConfigureAwait(false);
        }
    }
}
