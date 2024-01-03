// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.UrBox
{
    using Refit;

    public class UrBoxQueryModel
    {
        [AliasAs("app_secret")]
        public string? AppSecret { get; set; }

        [AliasAs("app_id")]
        public string? AppId { get; set; }
    }
}
