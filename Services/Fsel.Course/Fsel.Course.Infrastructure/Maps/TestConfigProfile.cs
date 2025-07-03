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
            CreateMap<TestConfig, TestConfigModel>()
                .ForMember(dest => dest.ProgramName, opt => opt.MapFrom(src => src.Program != null ? src.Program.Name : null))
                .ForMember(dest => dest.LevelName, opt => opt.MapFrom(src => src.Level != null ? src.Level.Name : null))
                .ForMember(dest => dest.SkillLevels, opt => opt.MapFrom(src =>
                    src.Level != null && src.Level.SkillLevels != null
                        ? src.Level.SkillLevels
                            .Where(z => z.Level != null)
                            .Select(z => z.Level!.Name)
                            .ToList()
                        : new List<string?>()))
                .ForMember(dest => dest.TestConfigSectionModels, opt => opt.Ignore());

            CreateMap<TestConfigSection, TestConfigSectionModel>()
                        .ForMember(dest => dest.Skill, opt => opt.Ignore())
                        .ForMember(dest => dest.Children, opt => opt.Ignore());


            CreateMap<CreateTestConfigCommandModel, TestConfig>().IgnoreAllNonExisting();
            CreateMap<CreateTestConfigSectionCommandModel, TestConfigSection>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TestConfigId, opt => opt.Ignore())
                .ForMember(dest => dest.ParentId, opt => opt.Ignore())
                .IgnoreAllNonExisting();
            CreateMap<CreateTestConfigCommandModel, TestConfig>()
                .ForMember(dest => dest.TestConfigSections, opt => opt.Ignore())
                .IgnoreAllNonExisting();


            CreateMap<UpdateTestConfigCommandModel, TestConfig>().IgnoreAllNonExisting();
            CreateMap<UpdateTestConfigSectionCommandModel, TestConfigSection>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TestConfigId, opt => opt.Ignore())
                .ForMember(dest => dest.ParentId, opt => opt.Ignore())
                .IgnoreAllNonExisting();
            CreateMap<UpdateTestConfigCommandModel, TestConfig>()
                .ForMember(dest => dest.TestConfigSections, opt => opt.Ignore())
                .IgnoreAllNonExisting();

        }
    }
}
