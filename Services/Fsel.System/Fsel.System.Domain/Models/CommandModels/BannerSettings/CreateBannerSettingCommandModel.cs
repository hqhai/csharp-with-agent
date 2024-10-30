// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.BannerSettings
{
    public class CreateBannerSettingCommandModel
    {
        public int MaximumPerDay { get; set; }

        public long DisplayIntervalTime { get; set; }
    }
}
