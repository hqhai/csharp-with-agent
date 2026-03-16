// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.ValueSettings
{
    using Fsel.Common.ValueSettings;

    /// <summary>
    /// AppSetting cho Realtime Service
    /// Mở rộng BaseAppSetting để thêm các config riêng cho Realtime
    /// </summary>
    public class AppSetting : BaseAppSetting
    {
        public AzureAiConfig? AzureAiConfig { get; set; }
        public new Services? Services { get; set; }
    }

    public class Services : BaseServices
    {
        public string? StorageApiUrl { get; set; }
        public string? SystemApiUrl { get; set; }
    }
}
