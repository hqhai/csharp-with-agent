// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService.Models.Request
{
    using System.Text.Json.Serialization;
    using Refit;
    using Fsel.Ordering.Infrastructure.ValueSettings;

    public class UrBoxAuthModel
    {
        public UrBoxAuthModel(AppSetting appSetting)
        {
            AppSecret = appSetting?.UrBoxConfig?.AppSecret;
            AppId = appSetting?.UrBoxConfig?.AppId;
        }

        [AliasAs("app_secret")]
        [JsonPropertyName("app_secret")]
        public string? AppSecret { get; set; }

        [AliasAs("app_id")]
        [JsonPropertyName("app_id")]
        public string? AppId { get; set; }
    }
}
