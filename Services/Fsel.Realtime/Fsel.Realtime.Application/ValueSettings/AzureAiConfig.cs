// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.ValueSettings
{
    /// <summary>
    /// Cấu hình Azure AI Services (Speech, Language, etc.)
    /// </summary>
    public class AzureAiConfig
    {
        /// <summary>
        /// API Key chính cho Azure Speech Service
        /// </summary>
        public string? FirstApiKey { get; set; }

        /// <summary>
        /// API Key phụ (fallback) cho Azure Speech Service
        /// </summary>
        public string? SecondApiKey { get; set; }

        /// <summary>
        /// Region/Vị trí của Azure resource (ví dụ: eastus, westus, japaneast)
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Ngôn ngữ mặc định cho Speech Recognition (ví dụ: en-US, ja-JP, vi-VN)
        /// </summary>
        public string? SpeechRecognitionLanguage { get; set; }
    }
}
