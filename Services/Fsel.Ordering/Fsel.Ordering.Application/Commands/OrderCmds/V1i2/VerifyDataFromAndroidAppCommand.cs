// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.V1i2
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Application.Services.InAppPurchase.Android;
    using Fsel.Ordering.Domain.Models.CommandModels.InAppPurchases.Androids;
    using Fsel.Shared.Constants;
    using MediatR;

    public class VerifyDataFromAndroidAppCommand : VerifyDataFromAndroidAppCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class VerifyDataFromAndroidAppCommandHandler : IRequestHandler<VerifyDataFromAndroidAppCommand, MethodResult<bool>>
    {
        private readonly IGooglePlayBillingService _service;

        public VerifyDataFromAndroidAppCommandHandler(IGooglePlayBillingService service)
        {
            _service = service;
        }

        public async Task<MethodResult<bool>> Handle(VerifyDataFromAndroidAppCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.PackageName) || string.IsNullOrEmpty(request.ProductId) || string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.SubscriptionId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var verifyProduct = await _service.VerifySubscriptionAsync(request.PackageName, request.SubscriptionId, request.Token);
            if (verifyProduct == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
        }
    }
}
