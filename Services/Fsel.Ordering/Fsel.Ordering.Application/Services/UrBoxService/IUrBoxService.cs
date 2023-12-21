// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService
{
    using System.Threading.Tasks;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.QueryModels.UrBox;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IUrBoxService
    {
        [Get("/gift/lists")]
        Task<IApiResponse<UrBoxModel>> GetList([FromBody] GetTheGiftListFromUrBoxQueryModel query);

        [Get("/gift/detail")]
        Task<IApiResponse<GiftDetailModel>> Get([FromBody] GetTheGiftQueryModel query);

        [Get("/category/catbyparent")]
        Task<IApiResponse<CategoryModel>> GetListCategory([FromBody] GetListCategoryQueryModel query);

        [Get("/gift/brand")]
        Task<IApiResponse<BrandModel>> GetListBrand([FromBody] GetListBrandQueryModel query);
    }
}
