// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService.Models.Request
{
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Refit;

    public class GetTheGiftListFromUrBoxModel : UrBoxAuthModel
    {
        public GetTheGiftListFromUrBoxModel(AppSetting appSetting) : base(appSetting)
        {
        }

        [AliasAs("cat_id")]
        public int? CatId { get; set; }

        [AliasAs("lang")]
        public string? Language { get; set; }
    }
}
