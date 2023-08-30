// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Infrastructure.ValueSettings
{
    using Fsel.Common.ValueSettings;

    public class AppSetting : BaseAppSetting
    {
        public new Services? Services { get; set; }
    }

    public class Services : BaseServices
    {

    }
}
