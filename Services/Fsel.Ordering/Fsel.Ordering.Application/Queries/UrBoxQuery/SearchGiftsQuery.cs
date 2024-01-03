// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UrBoxQuery
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Domain.Models.EntityModels.UrBox;
    using Fsel.Ordering.Domain.Models.QueryModels.UrBox;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SearchGiftsQuery : SearchGiftQueryModel, IRequest<MethodResult<PagingItemsModel<GiftModel>>>
    {
    }

    public class SearchGiftsQueryHandler : IRequestHandler<SearchGiftsQuery, MethodResult<PagingItemsModel<GiftModel>>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;

        public SearchGiftsQueryHandler(IUrBoxService urBoxService, AppSetting appSetting)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<PagingItemsModel<GiftModel>>> Handle(SearchGiftsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<GiftModel>>();

            var getAllGift = await _urBoxService.GetAllGift(new GetTheGiftListFromUrBoxModel
            {
                AppSecret = _appSetting.UrBoxConfig?.AppSecret,
                AppId = _appSetting.UrBoxConfig?.AppId,
                CatId = request.CategoryId,
            });

            var theGiftList = getAllGift.Content;

            if (theGiftList?.Status != 200)
            {
                methodResult.AddErrorBadRequest(theGiftList?.Msg);
                return methodResult;
            }

            if (theGiftList.Data?.Items?.Count > 0)
            {
                var data = theGiftList.Data.Items;

                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    data = data.Where(p => !string.IsNullOrEmpty(p.Title) && p.Title.ToLower(CultureInfo.CurrentCulture) == request.Keyword.ToLower(CultureInfo.CurrentCulture)).ToList();
                }

                if (request.Min.HasValue && request.Max.HasValue)
                {
                    data = data.Where(p => long.TryParse(p.Price, out long priceValue) && priceValue >= request.Min && priceValue <= request.Max).ToList();
                }

                int totalItem = data.Count;
                var lists = data
                        .ApplySortAndPaging(request)
                        .ToList();
                methodResult.Result = new PagingItemsModel<GiftModel>(lists, request, totalItem);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            return methodResult;
        }
    }
}
