// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Models.CommandModels.RubyText;
    using Fsel.Course.Domain.Models.EntityModels;
    using EntityRubyText = Fsel.Course.Domain.Entities.RubyText;

    public class RubyTextProfile : Profile
    {
        public RubyTextProfile()
        {
            CreateMap<EntityRubyText, RubyTextModel>().IgnoreAllNonExisting();
            CreateMap<EntityRubyText, EntityRubyText>()
                .ForMember(m => m.BaseText, opt => opt.Ignore())
                .ForMember(m => m.Phonetic, opt => opt.Ignore())
                .ForMember(m => m.LanguageType, opt => opt.Ignore())
                .IgnoreAllNonExisting();
            CreateMap<CreateRubyTextCommandModel, EntityRubyText>().IgnoreAllNonExisting();
            CreateMap<UpdateRubyTextCommandModel, EntityRubyText>().IgnoreAllNonExisting();
        }
    }
}
