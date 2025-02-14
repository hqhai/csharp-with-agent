// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.MaintainConfigs
{
    using global::System;

    public class CreatePopupMaintainCommandModel
    {
        public string? Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NotificationTime { get; set; }
        public string? Content { get; set; }
        public string? Code { get; set; }
    }
}
