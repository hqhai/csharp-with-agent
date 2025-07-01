// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities.TestConfig;
    using Fsel.Course.Domain.Models.CommandModels.TestConfigs;
    using Fsel.Course.Domain.Models.CommandModels.TestConfigSections;
    using Fsel.Course.Domain.Models.EntityModels.TestConfig;

    public class TestConfigProfile : Profile
    {
        public TestConfigProfile()
        {
            CreateMap<TestConfig, TestConfigModel>().IgnoreAllNonExisting();
            CreateMap<CreateTestConfigCommandModel, TestConfig>().IgnoreAllNonExisting();
            CreateMap<CreateTestConfigSectionCommandModel, TestConfigSection>().IgnoreAllNonExisting();

            CreateMap<CreateTestConfigCommandModel, TestConfig>()
                .ForMember(dest => dest.TestConfigSections, opt => opt.Ignore())
                .IgnoreAllNonExisting();


            CreateMap<UpdateTestConfigCommandModel, TestConfig>().IgnoreAllNonExisting();
            CreateMap<UpdateTestConfigSectionCommandModel, TestConfigSection>().IgnoreAllNonExisting();

            CreateMap<UpdateTestConfigCommandModel, TestConfig>()
                .ForMember(dest => dest.TestConfigSections, opt => opt.Ignore())
                .IgnoreAllNonExisting();

        }
    }
}
