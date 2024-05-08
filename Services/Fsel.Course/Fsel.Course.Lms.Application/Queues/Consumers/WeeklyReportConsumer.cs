// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Commands.WeeklyReportCommand;
    using MassTransit;
    using MediatR;

    public class WeeklyReportConsumer : Core.Base.Interfaces.IBaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public WeeklyReportConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<Core.Base.BaseModels.BaseQueueDataModel<BaseQueueModel>> context)
        {
            await _mediator.Send(new WeeklyReportCommand()).ConfigureAwait(false);
        }
    }
}
