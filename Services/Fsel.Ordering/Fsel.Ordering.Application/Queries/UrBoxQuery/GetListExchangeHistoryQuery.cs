// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UrBoxQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Request;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Response;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using MediatR;

    public class GetListExchangeHistoryQuery : IRequest<MethodResult<ExchangeHistoryModel>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetListExchangeHistoryQueryHandler : IRequestHandler<GetListExchangeHistoryQuery, MethodResult<ExchangeHistoryModel>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;
        private readonly AuthContext _authContext;

        public GetListExchangeHistoryQueryHandler(IUrBoxService urBoxService, AppSetting appSetting, AuthContext authContext)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
            _authContext = authContext;
        }

        public async Task<MethodResult<ExchangeHistoryModel>> Handle(GetListExchangeHistoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExchangeHistoryModel>();

            var giftExchangeHistoryResult = await _urBoxService.GetGiftExchangeHistory(new GetGiftExchangeHistoryModel(_appSetting)
            {
                SiteUserId = _authContext.CurrentUserId.ToString(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
            });

            var giftExchangeHistory = giftExchangeHistoryResult.Content?.Data;

            var unused = giftExchangeHistory?.Where(p => p.PayStatusCode == 2).Where(n => n.Detail?.Count > 0).SelectMany(p => p.Detail!).Where(x => x.UsageStatusCode == 1).ToList();
            var used = giftExchangeHistory?.Where(p => p.PayStatusCode == 2).Where(n => n.Detail?.Count > 0).SelectMany(p => p.Detail!).Where(x => x.UsageStatusCode != 1).ToList();

            methodResult.Result = new ExchangeHistoryModel { Used = used, UnUsed = unused };
            return methodResult;
        }
    }
}
