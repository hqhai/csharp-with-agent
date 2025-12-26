// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Core.Extensions;
    using Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.AiPromptManager;
    using Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels;

    public class AiPromptManagerProfile : Profile
    {
        public AiPromptManagerProfile()
        {
            CreateMap<AiPromptManager, AiPromptManagerModel>().IgnoreAllNonExisting();
            CreateMap<CreateAiPromptManagerCommandModel, AiPromptManager>().IgnoreAllNonExisting();
            CreateMap<UpdateAiPromptCommandManagerModel, AiPromptManager>().IgnoreAllNonExisting();
        }
    }
}
