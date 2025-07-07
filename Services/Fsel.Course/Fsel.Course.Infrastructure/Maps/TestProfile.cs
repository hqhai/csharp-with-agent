// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using Fsel.Course.Domain.Models.CommandModels.TestSections;
    using Fsel.Course.Domain.Models.EntityModels.TestModels;

    public class TestProfile : Profile
    {
        public TestProfile()
        {
            CreateMap<Test, TestModel>().IgnoreAllNonExisting();
            CreateMap<TestSection, TestSectionModel>().IgnoreAllNonExisting();

            CreateMap<CreateTestSectionCommandModel, TestSection>().IgnoreAllNonExisting();
            CreateMap<CreateTestCommandModel, Test>().ForMember(dest => dest.TestSections, opt => opt.Ignore()).IgnoreAllNonExisting();

            CreateMap<UpdateTestSectionCommandModel, TestSection>().IgnoreAllNonExisting();
            CreateMap<UpdateTestCommandModel, Test>().ForMember(dest => dest.TestSections, opt => opt.Ignore()).IgnoreAllNonExisting();
        }
    }
}
