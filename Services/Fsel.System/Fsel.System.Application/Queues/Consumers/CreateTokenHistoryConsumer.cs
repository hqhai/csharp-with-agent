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
            var message = context?.Message;
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new TokenHistoryQueueModel
            {
                ObjectId = message.ObjectId,
                InitialToken = message.InitialToken,
                RemainToken = message.RemainToken,
                VolatileToken = message.VolatileToken,
                Mission = message.Mission,
                Feature = message.Feature,
                Type = message.Type,
                UserId = message.UserId
            }).ConfigureAwait(false);
        }
    }
}
