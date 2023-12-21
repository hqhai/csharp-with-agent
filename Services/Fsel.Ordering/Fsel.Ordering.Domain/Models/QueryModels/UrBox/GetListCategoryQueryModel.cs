// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.UrBox
{
    using Refit;

    public class GetListCategoryQueryModel
    {
        [AliasAs("app_secret")]
        public string? AppSecret { get; set; }

        [AliasAs("app_id")]
        public int AppId { get; set; }

        [AliasAs("parent_id")]
        public int? ParentId { get; set; }

        [AliasAs("lang")]
        public string? Language { get; set; }
    }
}
