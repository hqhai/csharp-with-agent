// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UrBoxQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.QueryModels.UrBox;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using MediatR;

    public class GetTheGiftQuery : IRequest<MethodResult<GiftDetailModel>>
    {
        public string? Id { get; set; }
        public string? Language { get; set; }
    }

    public class GetTheGiftQueryHandler : IRequestHandler<GetTheGiftQuery, MethodResult<GiftDetailModel>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;

        public GetTheGiftQueryHandler(IUrBoxService urBoxService, AppSetting appSetting)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<GiftDetailModel>> Handle(GetTheGiftQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<GiftDetailModel>();

            var theGift = await _urBoxService.Get(new GetTheGiftQueryModel
            {
                AppSecret = _appSetting.UrBoxConfig?.AppSecret,
                AppId = _appSetting.UrBoxConfig?.AppId ?? 20,
                Id = request.Id,
                Language = request.Language,
            });
            if (!theGift.IsSuccessStatusCode)
            {
                methodResult.AddError(theGift.Error);
                return methodResult;
            }
            methodResult.Result = theGift.Content;
            return methodResult;
        }
    }
}
