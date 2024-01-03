// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.UrBox
{
    using Refit;

    public class GetListBrandQueryModel : UrBoxQueryModel
    {
        [AliasAs("cat_id")]
        public int? CategoryId { get; set; }
    }
}
