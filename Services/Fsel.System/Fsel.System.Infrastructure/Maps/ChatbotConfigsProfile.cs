// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities.Chatbots;
    using Fsel.System.Domain.Models.CommandModels.ChatbotConfigs;
    public class ChatbotConfigsProfile : Profile
    {
        public ChatbotConfigsProfile()
        {
            CreateMap<SaveChatbotConfigCommandModel, ChatbotConfig>().IgnoreAllNonExisting();
        }
    }
}
