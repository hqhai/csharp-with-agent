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
        /// <summary>
        /// Cấu hình Azure AI Services (Speech-to-Text, etc.)
        /// </summary>
        public AzureAiConfig? AzureAiConfig { get; set; }
    }
}
