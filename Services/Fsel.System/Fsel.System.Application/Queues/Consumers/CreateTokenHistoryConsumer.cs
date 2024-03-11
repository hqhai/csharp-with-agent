// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Consumers
{
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.TokenHistoryCmd;
    using global::System.Threading.Tasks;
    using MassTransit;
    using MediatR;

    public class CreateTokenHistoryConsumer : IConsumer<TokenHistoryQueueModel>
    {
        private readonly IMediator _mediator;

        public CreateTokenHistoryConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<TokenHistoryQueueModel> context)
        {
            var data = context?.Message;
            if (data != null)
            {
                await _mediator.Send(new CreateTokenHistoryCommand
                {
                    ObjectId = data.ObjectId,
                    InitialToken = data.InitialToken,
                    TokenConfigId = data.TokenConfigId,
                    RemainToken = data.RemainToken,
                    VolatileToken = data.VolatileToken,
                    Type = data.Type,
                    UserId = data.UserId
                });
            }
        }
    }
}
