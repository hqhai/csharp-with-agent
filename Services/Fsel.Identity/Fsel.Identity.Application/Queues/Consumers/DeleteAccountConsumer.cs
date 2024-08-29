// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Commands.StudentRankingCmd;
    using Fsel.Identity.Application.Commands.UserDeletionCmd;
    using MediatR;

    public class DeleteAccountConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public DeleteAccountConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(BaseQueueModel? message)
        {
            await _mediator.Send(new DeleteUserToDeleteAccountCommand()).ConfigureAwait(false);
        }
    }
}
