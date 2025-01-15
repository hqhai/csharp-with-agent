// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Domain.Models.EntityModels
{
    public class TranscriptFileModel
    {
        public string? FilePath { get; set; }
        public string? Content { get; set; }
    }

    public class ContentModel
    {
        public string? Text { get; set; }
    }
}
