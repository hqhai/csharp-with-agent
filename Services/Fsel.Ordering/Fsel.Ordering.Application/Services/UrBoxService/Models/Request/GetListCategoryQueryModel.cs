// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService.Models.Request
{
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Refit;

    public class GetListCategoryQueryModel : UrBoxAuthModel
    {
        public GetListCategoryQueryModel(AppSetting appSetting) : base(appSetting)
        {
        }

        [AliasAs("parent_id")]
        public int? ParentId { get; set; }

        [AliasAs("lang")]
        public string? Language { get; set; }
    }
}
