namespace Fsel.System.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.TokenHistoryCmd;
    using MediatR;

    public class AddCoinWhenCoursePurchasedConsumer : BaseConsumer<AddCoinWhenCoursePurchasedCommandModel>
    {
        private readonly IMediator _mediator;

        public AddCoinWhenCoursePurchasedConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(AddCoinWhenCoursePurchasedCommandModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new AddCoinWhenCoursePurchasedCommand
            {
                UserIds = message.UserIds,
                Coins = message.Coins,
                Month = message.Month,
                ObjectId = message.ObjectId
            }).ConfigureAwait(false);
        }
    }
}
