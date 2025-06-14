namespace Fsel.Ordering.Application.Commands.MarketplacePremiumCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Response;
    using Fsel.Ordering.Domain.Models.CommandModels.MarketPlacePremium;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class SpamRedeemProductPremiumCommand : RedeemProductPremiumCommandModel, IRequest<MethodResult<RedemptionResponseModel>>
    {
    }

    public class SpamRedeemProductPremiumCommandHandler : IRequestHandler<SpamRedeemProductPremiumCommand, MethodResult<RedemptionResponseModel>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SpamRedeemProductPremiumCommandHandler> _logger;

        public SpamRedeemProductPremiumCommandHandler(IMediator mediator, ILogger<SpamRedeemProductPremiumCommandHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<MethodResult<RedemptionResponseModel>> Handle(SpamRedeemProductPremiumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<RedemptionResponseModel>();

            var tasks = new List<Task>();

            var range = Enumerable.Range(0, 10);
            object consoleLock = new();
            await Parallel.ForEachAsync(range, cancellationToken, async (i, ct) =>
            {
                try
                {
                    Console.WriteLine($"Redeem #{i + 1} time: {DateTime.Now.ToString()}");
                    var result = await _mediator.Send(new RedeemProductPremiumCommand()
                    {
                        ProductId = request.ProductId,
                        PhoneNumber = request.PhoneNumber
                    }, ct);

                    if (!result.IsOK)
                    {
                    }

                    _logger.LogError($"Redeem #{i + 1} completed: {result.Serialize()}");

                    lock (consoleLock)
                    {
                        Console.WriteLine($"Redeem #{i + 1} completed: {result.Serialize()}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Redeem #{i + 1} failed");
                    lock (consoleLock)
                    {
                        Console.WriteLine($"Redeem #{i + 1} failed");
                    }
                }
            });

            return methodResult;
        }
    }
}
