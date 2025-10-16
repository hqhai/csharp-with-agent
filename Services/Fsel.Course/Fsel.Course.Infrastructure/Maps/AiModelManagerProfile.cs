// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Core.Extensions;
    using Domain.Entities;
    using Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.CommandModels.AiModelManager;

    public class AiModelManagerProfile : Profile
    {
        public AiModelManagerProfile()
        {
            CreateMap<AiModelManager, AiManagerModel>().IgnoreAllNonExisting();
            CreateMap<CreateAiModelManagerCommandModel, AiModelManager>().IgnoreAllNonExisting();
            CreateMap<UpdateAiModelCommandManagerModel, AiModelManager>().IgnoreAllNonExisting();
        }
    }
}
