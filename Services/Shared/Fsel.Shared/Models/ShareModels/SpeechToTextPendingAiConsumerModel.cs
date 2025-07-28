// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    public class SpeechToTextPendingAiConsumerModel
    {
        public Guid UserId { get; set; }

        public Guid ClassForumDetailResultId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public byte[] FileData { get; set; } = Array.Empty<byte>();
    }
}
