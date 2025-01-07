// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class BannerSettingModel : BaseModel
    {
        public int MaximumPerDay { get; set; }

        public long DisplayIntervalTime { get; set; }

        public long TimeSlideShow { get; set; }
    }
}
