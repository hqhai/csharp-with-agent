// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.UrBox
{
    using Refit;

    public class GetListBrandQueryModel
    {
        [AliasAs("app_secret")]
        public string? AppSecret { get; set; }

        [AliasAs("app_id")]
        public int AppId { get; set; }

        [AliasAs("cat_id")]
        public int? CategoryId { get; set; }
    }
}
