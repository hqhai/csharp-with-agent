// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Consumers
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.TokenHistoryCmd;
    using global::System.Threading.Tasks;
    using MassTransit;
    using MediatR;

    public class CreateTokenHistoryConsumer : Core.Base.Interfaces.IBaseConsumer<TokenHistoryQueuesModel>
    {
        private readonly IMediator _mediator;

        public CreateTokenHistoryConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<BaseQueueDataModel<TokenHistoryQueuesModel>> context)
        {
            var message = context?.Message;
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new CreateTokenHistoryCommand
            {
                TokenHistorys = message.Data?.TokenHistories
            }).ConfigureAwait(false);
        }
    }
}
