// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Commands.StudentGoalAggregateCmd;
    using MediatR;

    public class NotifyWeeklyCourseGoalTargetConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public NotifyWeeklyCourseGoalTargetConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(BaseQueueModel? message)
        {
            await _mediator.Send(new SendWeeklyCourseGoalTargetCommand()).ConfigureAwait(false);
        }
    }
}
