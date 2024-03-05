// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService
{
    using System.Threading.Tasks;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Request;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Response;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IUrBoxService
    {
        [Get("/4.0/gift/lists")]
        Task<IApiResponse<GetAllGiftModel>> GetAllGift([FromBody] GetTheGiftListFromUrBoxModel model);

        [Get("/4.0/gift/detail")]
        Task<IApiResponse<GetGiftDetailModel>> Get([FromBody] GetTheGiftQueryModel model);

        [Get("/2.0/category/catbyparent")]
        Task<IApiResponse<GetListCategoryModel>> GetListCategory([FromBody] GetListCategoryQueryModel model);

        [Get("/4.0/gift/brand")]
        Task<IApiResponse<BrandModel>> GetListBrand([FromBody] GetListBrandQueryModel model);

        [Post("/2.0/cart/cartPayVoucher")]
        Task<IApiResponse<RedemptionResponseModel>> CreateRedemptionRequest([Body] CreateRedemptionRequestModel model, [Header("Signature")] string signature);

        [Get("/2.0/cart/getlist")]
        Task<IApiResponse<GiftExchangeHistoryModel>> GetGiftExchangeHistory([FromBody] GetGiftExchangeHistoryModel model);

        [Get("/2.0/cart/getByTransaction")]
        Task<IApiResponse<DetailExchangeHistoryModel>> GetDetailExchangeHistory([FromBody] GetDetailExchangeHistoryModel model);
    }
}
