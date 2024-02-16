// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Queries.WeeklyReportQuery;
    using MassTransit;
    using MediatR;

    public class WeeklyReportConsumer : IConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public WeeklyReportConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<BaseQueueModel> context)
        {
            await _mediator.Send(new WeeklyReportQuery()).ConfigureAwait(false);
        }
    }
}
