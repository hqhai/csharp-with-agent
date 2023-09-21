// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Queries.ClassForumResultQuery;
    using MassTransit;
    using MassTransit.Mediator;

    public class UpdateClassForumResultTime : IConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public UpdateClassForumResultTime(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<BaseQueueModel> context)
        {
            await _mediator.Send(new GetClassForumResultQuery()).ConfigureAwait(false);
        }
    }
}
