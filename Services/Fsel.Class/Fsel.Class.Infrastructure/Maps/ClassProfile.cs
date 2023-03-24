// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Class.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Class.Doman.Models.CommandModels.Classes;
    using Fsel.Class.Doman.Models.EntityModels;
    using Classes = Fsel.Class.Doman.Entities.Class;
    using Fsel.Core.Extensions;

    public class ClassProfile : Profile
    {
        public ClassProfile()
        {
            CreateMap<Classes, ClassModel>().IgnoreAllNonExisting();
            CreateMap<CreateClassCommandModel, Classes>().IgnoreAllNonExisting();
        }
    }
}
