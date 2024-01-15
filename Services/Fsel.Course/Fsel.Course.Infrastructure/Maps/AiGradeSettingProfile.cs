// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.AiGradeSetting;

namespace Fsel.Course.Infrastructure.Maps
{
    public class AiGradeSettingProfile : Profile
    {
        public AiGradeSettingProfile()
        {
            CreateMap<AiGradeSettingModel, AiGradeSetting>().IgnoreAllNonExisting();
        }
    }
}
