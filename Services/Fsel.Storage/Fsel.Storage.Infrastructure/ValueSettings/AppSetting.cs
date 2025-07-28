// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Infrastructure.ValueSettings
{
    using Fsel.Common.ValueSettings;

    public class AppSetting : BaseAppSetting
    {
        public new StorageConfig? StorageConfig { get; set; }
        public OpenAiConfig? OpenAiConfig { get; set; }
        public new Services? Services { get; set; }
    }

    public class Services : BaseServices
    {
        public string? FFmpegApiUrl { get; set; }
    }

    public class OpenAiConfig
    {
        public string? Uri { get; set; }
        public IList<string>? ApiKeys { get; set; }
        public string? ApprovalAIModel { get; set; }
        public string? Language { get; set; }
    }

    public class StorageFolder
    {
        public string? Videos { get; set; }
        public string? Questions { get; set; }
        public string? Files { get; set; }
    }

    public class StorageConfig
    {
        public string? BucketName { get; set; }
        public string? AwsS3BaseUrl { get; set; }
        public string? AwsAccessKey { get; set; }
        public string? AwsSecretAccessKey { get; set; }
        public StorageFolder? Folders { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(BucketName ?? AwsS3BaseUrl ?? AwsAccessKey ?? AwsSecretAccessKey);
        }
    }
}
