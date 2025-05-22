// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    public class SpeechToTextConsumerModel
    {
        public Guid UserId { get; set; }

        public TranscriptFileModel? TranscriptFile { get; set; }
    }

    public class TranscriptFileModel
    {
        public string? FilePath { get; set; }
        public string? Content { get; set; }
        public string? DateTime { get; set; }
    }
}
