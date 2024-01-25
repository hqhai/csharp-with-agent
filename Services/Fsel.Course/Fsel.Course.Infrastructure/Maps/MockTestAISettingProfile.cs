// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.AiGradeSetting;
using Fsel.Course.Domain.Models.CommandModels.Sections;

namespace Fsel.Course.Infrastructure.Maps
{
    public class MockTestAISettingProfile : Profile
    {
        public MockTestAISettingProfile()
        {
            CreateMap<SectionAiSettingModel, MockTestAISetting>().IgnoreAllNonExisting();
            CreateMap<MockTestAISettingModel, SectionAiSettingModel>().IgnoreAllNonExisting();
            CreateMap<MockTestAISetting, MockTestAISettingModel>().IgnoreAllNonExisting();
        }
    }
}
