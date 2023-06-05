// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Models.CommandModels.Actions;

    public class InteractionActionProfile : Profile
    {
        public InteractionActionProfile()
        {
            CreateMap<CreateActionCommandModel, InteractionAction>().IgnoreAllNonExisting();
        }
    }
}
