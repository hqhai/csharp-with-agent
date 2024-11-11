// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.MaintainConfigs
{
    using global::System;

    public class SetTimePopupMaintainCommandModel
    {
        public Guid BannerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NotificationTime { get; set; }
    }
}
