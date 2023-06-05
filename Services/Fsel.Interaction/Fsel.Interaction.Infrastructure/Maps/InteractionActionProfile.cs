// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Maps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Models.CommandModels.Actions;
    using Fsel.Interaction.Domain.Models.EntityModels;

    public class InteractionActionProfile : Profile
    {
        public InteractionActionProfile()
        {
            CreateMap<InteractionActions, InteractionActionModel>().IgnoreAllNonExisting();
            CreateMap<CreateActionCommandModel, InteractionActions>().IgnoreAllNonExisting();
        }
    }
}
