// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.StorageServices.Models
{
    public class ConvertSpeechToTextModel
    {
        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public byte[] FileData { get; set; } = Array.Empty<byte>();
    }
}
