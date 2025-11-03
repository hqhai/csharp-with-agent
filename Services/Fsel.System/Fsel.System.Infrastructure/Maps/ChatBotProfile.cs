// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities.ChatBot;
    using Fsel.System.Domain.Models.CommandModels.ChatBot;
    using Fsel.System.Domain.Models.EntityModels;

    public class ChatBotProfile : Profile
    {
        public ChatBotProfile()
        {
            CreateMap<ChatBotMessageModel, ChatBotMessage>().IgnoreAllNonExisting();
            CreateMap<ChatBotMessageModel, ChatbotResponseModel>().IgnoreAllNonExisting();
            CreateMap<ChatbotResponseModel, ChatBotMessage>().IgnoreAllNonExisting();

            CreateMap<ChatBotMessage, ChatBotMessageModel>().IgnoreAllNonExisting();
            CreateMap<ChatBotMessage, ChatbotResponseModel>().IgnoreAllNonExisting();
            CreateMap<SaveChatBotMessageModel, ChatBot>().IgnoreAllNonExisting();
            CreateMap<ChatBot, ChatBotModel>().IgnoreAllNonExisting();
        }
    }
}
