// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Class.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Class.Doman.Models.EntityModels;
    using Fsel.Core.Extensions;
    using Classes = Fsel.Class.Doman.Entities.Class;

    public class ClassProfile : Profile
    {
        public ClassProfile()
        {
            CreateMap<Classes, ClassModel>().IgnoreAllNonExisting();
        }
    }
}
