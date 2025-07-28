namespace Fsel.System.Application.Commands.BlindBoxes
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Application.Queues.Publisher;
    using Fsel.System.Domain.Models.CommandModels.BlindBoxes;
    using MediatR;

    public class BuyBlindBoxConsumerCommand : BuyBlindBoxCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class BuyBlindBoxConsumerCommandHandler : IRequestHandler<BuyBlindBoxConsumerCommand, MethodResult<bool>>
    {
        private readonly BuyBlindBoxPublisher _buyBlindBoxPublisher;

        public BuyBlindBoxConsumerCommandHandler(BuyBlindBoxPublisher buyBlindBoxPublisher)
        {
            _buyBlindBoxPublisher = buyBlindBoxPublisher;
        }

        public async Task<MethodResult<bool>> Handle(BuyBlindBoxConsumerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            await _buyBlindBoxPublisher.Publish(new BuyBlindBoxCommandModel
            {
                BlindBoxChestId = request.BlindBoxChestId
            }, cancellationToken);

            methodResult.Result = true;
            return methodResult;
        }
    }
}
