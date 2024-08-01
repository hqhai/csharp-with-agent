// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queues.Consumers
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using MediatR;

    public class DeleteGuestStudentConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public DeleteGuestStudentConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(BaseQueueModel? message)
        {
            if (message == null)
            {
                return;
            }

            var classForum = new DeleteGuestStudentByUserIdCommand
            {
                Id = Guid.Parse(message.QueueId!),
            };
            await _mediator.Send(classForum).ConfigureAwait(false);
        }
    }
}
