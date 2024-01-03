// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UrBoxQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Domain.Models.EntityModels.UrBox;
    using Fsel.Ordering.Domain.Models.QueryModels.UrBox;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using MediatR;

    public class GetGiftQuery : IRequest<MethodResult<GiftDetailModel>>
    {
        public string? Id { get; set; }
        public string? Language { get; set; }
    }

    public class GetTheGiftQueryHandler : IRequestHandler<GetGiftQuery, MethodResult<GiftDetailModel>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;

        public GetTheGiftQueryHandler(IUrBoxService urBoxService, AppSetting appSetting)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<GiftDetailModel>> Handle(GetGiftQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<GiftDetailModel>();

            var theGiftResult = await _urBoxService.Get(new GetTheGiftQueryModel
            {
                AppSecret = _appSetting.UrBoxConfig?.AppSecret,
                AppId = _appSetting.UrBoxConfig?.AppId,
                Id = request.Id,
                Language = request.Language,
            });

            var theGift = theGiftResult.Content;
            if (theGift?.Status != 200)
            {
                methodResult.AddErrorBadRequest(theGift?.Msg);
                return methodResult;
            }
            methodResult.Result = theGift.Data;
            return methodResult;
        }
    }
}
