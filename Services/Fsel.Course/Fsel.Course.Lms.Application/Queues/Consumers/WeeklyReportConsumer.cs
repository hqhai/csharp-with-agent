// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Commands.WeeklyReportCommand;
    using MassTransit;
    using MediatR;

    public class WeeklyReportConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public WeeklyReportConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(BaseQueueModel? message)
        {
            await _mediator.Send(new WeeklyReportCommand()).ConfigureAwait(false);
        }
    }
}
