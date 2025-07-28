namespace Fsel.System.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.System.Application.Commands.BlindBoxes;
    using Fsel.System.Domain.Models.CommandModels.BlindBoxes;
    using MediatR;

    public class BuyBlindBoxConsumer : BaseConsumer<BuyBlindBoxCommandModel>
    {
        private readonly IMediator _mediator;

        public BuyBlindBoxConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(BuyBlindBoxCommandModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new BuyBlindBoxCommand
            {
                BlindBoxChestId = message.BlindBoxChestId
            }).ConfigureAwait(false);
        }
    }
}
