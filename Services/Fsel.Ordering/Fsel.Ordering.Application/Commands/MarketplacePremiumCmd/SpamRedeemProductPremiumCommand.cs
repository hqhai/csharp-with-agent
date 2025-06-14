namespace Fsel.Ordering.Application.Commands.MarketplacePremiumCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Response;
    using Fsel.Ordering.Domain.Models.CommandModels.MarketPlacePremium;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class SpamRedeemProductPremiumCommand : RedeemProductPremiumCommandModel, IRequest<MethodResult<RedemptionResponseModel>>
    {
    }

    public class SpamRedeemProductPremiumCommandHandler : IRequestHandler<SpamRedeemProductPremiumCommand, MethodResult<RedemptionResponseModel>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SpamRedeemProductPremiumCommandHandler> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public SpamRedeemProductPremiumCommandHandler(IMediator mediator, ILogger<SpamRedeemProductPremiumCommandHandler> logger, IServiceScopeFactory scopeFactory)
        {
            _mediator = mediator;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task<MethodResult<RedemptionResponseModel>> Handle(SpamRedeemProductPremiumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<RedemptionResponseModel>();

            var tasks = new List<Task>();

            var range = Enumerable.Range(0, 50);
            object consoleLock = new();
            await Parallel.ForEachAsync(range, cancellationToken, async (i, ct) =>
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                try
                {
                    Console.WriteLine($"Redeem #{i + 1} time: {DateTime.Now.ToString()}");
                    var result = await mediator.Send(new RedeemProductPremiumCommand()
                    {
                        ProductId = request.ProductId,
                        PhoneNumber = request.PhoneNumber
                    }, ct);

                    if (!result.IsOK)
                    {
                        _logger.LogError($"Redeem #{i + 1} time: {DateTime.Now.ToString()} error: {result.Serialize()}");
                    }
                    else
                    {
                        _logger.LogError($"Redeem #{i + 1} time: {DateTime.Now.ToString()} completed: {result.Serialize()}");
                    }

                    //lock (consoleLock)
                    //{
                    //    Console.WriteLine($"Redeem #{i + 1} completed: {result.Serialize()}");
                    //}
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
