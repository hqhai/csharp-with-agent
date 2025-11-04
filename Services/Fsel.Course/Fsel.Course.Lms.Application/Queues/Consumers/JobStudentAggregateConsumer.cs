// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Commands.OtherFeatureCmd;
    using MediatR;

    public class JobStudentAggregateConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public JobStudentAggregateConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(BaseQueueModel? message)
        {
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new RebuildLearningGoalAggregateCommand()).ConfigureAwait(false);
        }
    }
}
