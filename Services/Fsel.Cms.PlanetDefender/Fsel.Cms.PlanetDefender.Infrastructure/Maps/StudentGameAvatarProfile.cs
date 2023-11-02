// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Core.Extensions;

    public class StudentGameAvatarProfile : Profile
    {
        public StudentGameAvatarProfile()
        {
            CreateMap<StudentGameAvatar, StudentGameAvatarModel>().IgnoreAllNonExisting();
        }
    }
}
