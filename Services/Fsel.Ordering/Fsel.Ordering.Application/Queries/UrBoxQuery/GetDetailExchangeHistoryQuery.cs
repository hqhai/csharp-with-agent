// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UrBoxQuery
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Domain.Models.EntityModels.UrBox;
    using Fsel.Ordering.Domain.Models.QueryModels.UrBox;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using MediatR;

    public class GetDetailExchangeHistoryQuery : IRequest<MethodResult<DetailExchangeHistoryModel>>
    {
        public string? TransactionId { get; set; }
    }

    public class GetDetailExchangeHistoryQueryHandler : IRequestHandler<GetDetailExchangeHistoryQuery, MethodResult<DetailExchangeHistoryModel>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;

        public GetDetailExchangeHistoryQueryHandler(IUrBoxService urBoxService, AppSetting appSetting)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<DetailExchangeHistoryModel>> Handle(GetDetailExchangeHistoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<DetailExchangeHistoryModel>();

            var detailExchangeHistoryResult = await _urBoxService.GetDetailExchangeHistory(new GetDetailExchangeHistoryModel
            {
                AppSecret = _appSetting.UrBoxConfig?.AppSecret,
                AppId = _appSetting.UrBoxConfig?.AppId,
                TransactionId = request.TransactionId,
            });

            if (!detailExchangeHistoryResult.IsSuccessStatusCode)
            {
                methodResult.AddError(detailExchangeHistoryResult.Error);
                return methodResult;
            }
            methodResult.Result = detailExchangeHistoryResult.Content;
            return methodResult;
        }
    }
}
