// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.UrBox
{
    using Refit;

    public class GetTheGiftQueryModel
    {
        [AliasAs("app_secret")]
        public string? AppSecret { get; set; }

        [AliasAs("app_id")]
        public int AppId { get; set; }

        [AliasAs("id")]
        public string? Id { get; set; }

        [AliasAs("lang")]
        public string? Language { get; set; }
    }
}
