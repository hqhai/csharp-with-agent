// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.PlacementTestAnswers;
using Fsel.Course.Domain.Models.CommandModels.PlacementTests;
using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Infrastructure.Maps
{
    using Domain.Entities.TestConfigs;

    public class PlacementTestProfile : Profile
    {
        public PlacementTestProfile()
        {
            CreateMap<PlacementTest, PlacementTestModel>().IgnoreAllNonExisting();
            CreateMap<CreatePlacementTestCommandModel, PlacementTest>().IgnoreAllNonExisting();
            CreateMap<UpdatePlacementTestCommandModel, PlacementTest>().IgnoreAllNonExisting();
            CreateMap<PlacementTest, PlacementTestDtoModel>().IgnoreAllNonExisting();
            CreateMap<PlacementTestAnswer, AnswerModel>().IgnoreAllNonExisting();
            CreateMap<TestAnswer, AnswerModel>().IgnoreAllNonExisting();
            CreateMap<CreatePlacementTestAnswerCommandModel, PlacementTestAnswer>().IgnoreAllNonExisting();
        }
    }
}
