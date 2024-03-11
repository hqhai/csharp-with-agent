// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Consumers
{
    using Fsel.Shared.Models.ShareModels;
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
            if (context != null)
            {
                await _mediator.Send(new TokenHistoryQueueModel
                {
                    ObjectId = context.Message.ObjectId,
                    InitialToken = context.Message.InitialToken,
                    TokenConfigId = context.Message.TokenConfigId,
                    RemainToken = context.Message.RemainToken,
                    VolatileToken = context.Message.VolatileToken,
                    Type = context.Message.Type,
                    UserId = context.Message.UserId
                }).ConfigureAwait(false);
            }
        }
    }
}
