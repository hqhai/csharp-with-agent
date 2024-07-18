// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UrBoxQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Request;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Response;
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
        private readonly AuthContext _languageContext;

        public SearchGiftsQueryHandler(IUrBoxService urBoxService, AppSetting appSetting, AuthContext languageContext)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
            _languageContext = languageContext;
        }

        public async Task<MethodResult<PagingItemsModel<GiftModel>>> Handle(SearchGiftsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<GiftModel>>();

            var getAllGift = await _urBoxService.GetAllGift(new GetTheGiftListFromUrBoxModel(_appSetting)
            {
                AppSecret = _appSetting.UrBoxConfig?.AppSecret,
                AppId = _appSetting.UrBoxConfig?.AppId,
                CatId = request.CategoryId,
                Language = _languageContext.CurrentCountryInfo?.CultureCode?.Substring(0, 2)
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
                    data = data.Where(p => !string.IsNullOrEmpty(p.Title) && p.Title.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (request.Min.HasValue && request.Max.HasValue)
                {
                    data = data.Where(p => long.TryParse(p.Price, out long priceValue) && priceValue >= request.Min && priceValue <= request.Max).ToList();
                }
                else if (request.Min.HasValue)
                {
                    data = data.Where(p => long.TryParse(p.Price, out long priceValue) && priceValue >= request.Min).ToList();
                }
                else if (request.Max.HasValue)
                {
                    data = data.Where(p => long.TryParse(p.Price, out long priceValue) && priceValue <= request.Max).ToList();
                }

                if (request.PopularOrLatest.HasValue && request.PopularOrLatest == true)
                {
                    data = data.OrderByDescending(x => long.TryParse(x.View, out long viewValue) ? viewValue : 0).ToList();
                }
                else if (request.PopularOrLatest.HasValue && request.PopularOrLatest == false)
                {
                    data = data.OrderByDescending(x => long.TryParse(x.Id, out long id) ? id : 0).ToList();
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
