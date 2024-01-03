// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.UrBox
{
    using Refit;

    public class GetListCategoryQueryModel : UrBoxQueryModel
    {
        [AliasAs("parent_id")]
        public int? ParentId { get; set; }

        [AliasAs("lang")]
        public string? Language { get; set; }
    }
}
