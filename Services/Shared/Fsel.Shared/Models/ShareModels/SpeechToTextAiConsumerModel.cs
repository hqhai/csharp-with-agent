// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    public class SpeechToTextAiConsumerModel
    {
        public Guid UserId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public byte[] FileData { get; set; } = Array.Empty<byte>();

        public string? CurrentDate { get; set; }
    }
}
