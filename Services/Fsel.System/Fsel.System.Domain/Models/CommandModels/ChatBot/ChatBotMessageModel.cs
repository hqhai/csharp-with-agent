// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ChatBot
{
    public class ChatBotMessageModel
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
    }

    public class ChatbotResponseModel : ChatBotMessageModel
    {
        public string? FilePath { get; set; }
    }
}
