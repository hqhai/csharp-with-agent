// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.ForbiddenWords;
    using Fsel.System.Domain.Models.EntityModels;

    public class ForbiddenWordProfile : Profile
    {
        public ForbiddenWordProfile()
        {
            CreateMap<ForbiddenWord, ForbiddenWordModel>().IgnoreAllNonExisting();
            CreateMap<CreateForbiddenWordCommandModel, ForbiddenWord>().IgnoreAllNonExisting();
            CreateMap<UpdateForbiddenWordCommandModel, ForbiddenWord>().IgnoreAllNonExisting();
        }
    }
}
