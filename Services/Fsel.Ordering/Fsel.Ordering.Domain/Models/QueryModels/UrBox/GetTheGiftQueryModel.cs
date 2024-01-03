// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.UrBox
{
    using Refit;

    public class GetTheGiftQueryModel : UrBoxQueryModel
    {
        [AliasAs("id")]
        public string? Id { get; set; }

        [AliasAs("lang")]
        public string? Language { get; set; }
    }
}
