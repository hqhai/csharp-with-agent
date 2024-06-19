// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService.Models.Request
{
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Refit;

    public class GetListBrandQueryModel : UrBoxAuthModel
    {
        public GetListBrandQueryModel(AppSetting appSetting) : base(appSetting)
        {
        }

        [AliasAs("cat_id")]
        public int? CategoryId { get; set; }
    }
}
