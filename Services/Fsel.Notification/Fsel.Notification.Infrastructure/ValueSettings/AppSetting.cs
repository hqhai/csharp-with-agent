// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Infrastructure.ValueSettings
{
    using Fsel.Common.ValueSettings;

    public class AppSetting : BaseAppSetting
    {
        public new Services? Services { get; set; }
        public ConstantUrl? ConstantUrl { get; set; }
    }

    public class Services : BaseServices
    {

    }
    public class ConstantUrl
    {
        public string? LmsWebsiteDomain { get; set; }
        public string? LcmsWebsiteDomain { get; set; }
        public string? LmsAdminWebsiteDomain { get; set; }
    }
}
