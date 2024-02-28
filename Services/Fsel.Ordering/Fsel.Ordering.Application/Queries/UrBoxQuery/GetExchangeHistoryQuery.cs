// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UrBoxQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Request;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Response;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using MediatR;

    public class GetExchangeHistoryQuery : IRequest<MethodResult<DetailExchangeHistoryModel>>
    {
        public string? TransactionId { get; set; }
    }

    public class GetExchangeHistoryQueryHandler : IRequestHandler<GetExchangeHistoryQuery, MethodResult<DetailExchangeHistoryModel>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;

        public GetExchangeHistoryQueryHandler(IUrBoxService urBoxService, AppSetting appSetting)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<DetailExchangeHistoryModel>> Handle(GetExchangeHistoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<DetailExchangeHistoryModel>();

            var detailExchangeHistoryResult = await _urBoxService.GetDetailExchangeHistory(new GetDetailExchangeHistoryModel(_appSetting)
            {
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
