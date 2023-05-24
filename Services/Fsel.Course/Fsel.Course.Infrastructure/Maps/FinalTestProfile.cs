// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.FinalTests;
    using Fsel.Course.Domain.Models.EntityModels;

    public class FinalTestProfile : Profile
    {
        public FinalTestProfile()
        {
            CreateMap<FinalTest, FinalTestModel>().IgnoreAllNonExisting();
            CreateMap<CreateFinalTestCommandModel, FinalTest>().IgnoreAllNonExisting();
            CreateMap<UpdateFinalTestCommandModel, FinalTest>().IgnoreAllNonExisting();

            CreateMap<FinalTestResult, FinalTestResultModel>().IgnoreAllNonExisting();
        }
    }
}
