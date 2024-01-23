// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService.Models.Request
{
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Refit;

    public class GetTheGiftQueryModel : UrBoxAuthModel
    {
        public GetTheGiftQueryModel(AppSetting appSetting) : base(appSetting)
        {
        }

        [AliasAs("id")]
        public string? Id { get; set; }

        [AliasAs("lang")]
        public string? Language { get; set; }
    }
}
