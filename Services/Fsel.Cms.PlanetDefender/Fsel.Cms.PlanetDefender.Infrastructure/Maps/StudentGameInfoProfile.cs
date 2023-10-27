// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.StudentGameInfos;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.StudentGameInfos;
    using Fsel.Core.Extensions;

    public class StudentGameInfoProfile : Profile
    {
        public StudentGameInfoProfile()
        {
            CreateMap<StudentGameInfo, StudentGameInfoModel>().IgnoreAllNonExisting();
            CreateMap<ChooseStudentGenderCommandModel, StudentGameInfo>().IgnoreAllNonExisting();
            CreateMap<ChooseUserLevelCommandModel, StudentGameInfo>().IgnoreAllNonExisting();
            CreateMap<CreateNickNameStudentGameInfoCommandModel, StudentGameInfo>().IgnoreAllNonExisting();
        }
    }
}
