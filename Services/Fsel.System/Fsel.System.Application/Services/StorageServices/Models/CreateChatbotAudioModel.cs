// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.StorageServices.Models
{
    public class CreateChatbotAudioModel
    {
        public string? Text { get; set; }

        public string? Model { get; set; } = "tts-1";

        public string? Voice { get; set; } = "nova";
    }
}
