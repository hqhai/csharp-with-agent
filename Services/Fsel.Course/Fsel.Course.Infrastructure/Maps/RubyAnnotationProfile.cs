// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Models.CommandModels.RubyAnnotation;
    using Fsel.Course.Domain.Models.EntityModels;
    using EntityRubyText = Fsel.Course.Domain.Entities.RubyAnnotation;

    public class RubyAnnotationProfile : Profile
    {
        public RubyAnnotationProfile()
        {
            CreateMap<EntityRubyText, RubyAnnotationModel>().IgnoreAllNonExisting();
            CreateMap<EntityRubyText, EntityRubyText>()
                .ForMember(m => m.SelectedText, opt => opt.Ignore())
                .ForMember(m => m.Phonetic, opt => opt.Ignore())
                .ForMember(m => m.LanguageType, opt => opt.Ignore())
                .ForMember(m => m.StartGraphemeIndex, opt => opt.Ignore())
                .ForMember(m => m.LengthGraphemes, opt => opt.Ignore())
                .IgnoreAllNonExisting();
            CreateMap<CreateRubyAnnotationCommandModel, EntityRubyText>().IgnoreAllNonExisting();
            CreateMap<UpdateRubyAnnotationCommandModel, EntityRubyText>().IgnoreAllNonExisting();
        }
    }
}
