// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities.Chatbots;
    using Fsel.System.Domain.Models.CommandModels.ChatbotConfigs;
    using Fsel.System.Domain.Models.EntityModels;

    public class ChatbotConfigsProfile : Profile
    {
        public ChatbotConfigsProfile()
        {
            CreateMap<SaveChatbotConfigCommandModel, ChatbotConfig>().IgnoreAllNonExisting();
            CreateMap<ChatbotSkillConfigsCommandModel, ChatbotSkillConfig>().IgnoreAllNonExisting();
            CreateMap<ChatbotTokenConfigsCommandModel, ChatbotTokenConfigs>().IgnoreAllNonExisting();
            CreateMap<SkillConfigCommandModel, SkillConfig>().IgnoreAllNonExisting();
            CreateMap<ItemSkillContentCommandModel, ItemSkillContent>().IgnoreAllNonExisting();

            CreateMap<ChatbotSkillConfig, ChatbotSkillConfigModel>().IgnoreAllNonExisting();
            CreateMap<ChatbotTokenConfigs, ChatbotTokenConfigsModel>().IgnoreAllNonExisting();
            CreateMap<SkillConfig, SkillConfigModel>().IgnoreAllNonExisting();
            CreateMap<ItemSkillContent, ItemSkillContentModel>().IgnoreAllNonExisting();
            CreateMap<ChatbotConfig, ChatbotConfigModel>().IgnoreAllNonExisting();
        }
    }
}
