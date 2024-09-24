// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Commands.ClassForumResultCmd;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class DeleteClassForumByFlagConsumer : BaseConsumer<DeleteClassForumByFlagQueueModel>
    {
        private readonly IMediator _mediator;

        public DeleteClassForumByFlagConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(DeleteClassForumByFlagQueueModel? message)
        {
            if (message == null)
            {
                return;
            }

            var classForum = new DeleteClassForumByFlagCommand
            {
                Ids = message.ObjectIds,

            };
            await _mediator.Send(classForum).ConfigureAwait(false);
        }
    }
}
