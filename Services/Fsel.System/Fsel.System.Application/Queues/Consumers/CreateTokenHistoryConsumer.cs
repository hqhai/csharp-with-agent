// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.TokenHistoryCmd;
    using global::System.Threading.Tasks;
    using MassTransit;
    using MediatR;

    public class CreateTokenHistoryConsumer : BaseConsumer<TokenHistoryQueuesModel>
    {
        private readonly IMediator _mediator;

        public CreateTokenHistoryConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(TokenHistoryQueuesModel? message)
        {
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new CreateTokenHistoryCommand
            {
                TokenHistorys = message.TokenHistories
            }).ConfigureAwait(false);
        }
    }
}
