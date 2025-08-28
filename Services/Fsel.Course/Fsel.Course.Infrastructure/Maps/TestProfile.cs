// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Models.CommandModels.TestAiSettings;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using Fsel.Course.Domain.Models.CommandModels.TestSections;
    using Fsel.Course.Domain.Models.EntityModels.TestModels;

    public class TestProfile : Profile
    {
        public TestProfile()
        {
            CreateMap<Test, TestModel>().IgnoreAllNonExisting();
            CreateMap<TestSection, TestSectionModel>().IgnoreAllNonExisting();

            CreateMap<CreateTestSectionCommandModel, TestSection>().ForMember(dest => dest.TestAISettings, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<CreateTestCommandModel, Test>().ForMember(dest => dest.TestSections, opt => opt.Ignore()).IgnoreAllNonExisting();

            CreateMap<UpdateTestSectionCommandModel, TestSection>().ForMember(dest => dest.TestAISettings, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<UpdateTestCommandModel, Test>().ForMember(dest => dest.TestSections, opt => opt.Ignore())
                                                     .IgnoreAllNonExisting();

            CreateMap<CreateTestAICriteriaSettingCommandModel, TestAICriteriaSetting>().IgnoreAllNonExisting();
            CreateMap<CreateTestAISettingCommandModel, TestAISetting>().IgnoreAllNonExisting();

            CreateMap<UpdateTestAICriteriaSettingCommandModel, TestAICriteriaSetting>().IgnoreAllNonExisting();
            CreateMap<UpdateTestAISettingCommandModel, TestAISetting>().ForMember(dest => dest.TestAICriteriaSettings, opt => opt.Ignore()).IgnoreAllNonExisting();

            CreateMap<TestAISetting, TestAISettingModel>()
                .ForMember(x => x.TestAICriteriaSettings, p => p.MapFrom(o => o.TestAICriteriaSettings.OrderBy(x => x.CreatedDate)));
            CreateMap<TestAICriteriaSetting, TestAICriteriaSettingModel>().IgnoreAllNonExisting();
        }
    }
}
