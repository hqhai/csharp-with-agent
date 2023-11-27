// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;

    public class VideoTimeCodeResultProfile : Profile
    {
        public VideoTimeCodeResultProfile()
        {
            CreateMap<VideoTimeCodeResult, VideoTimeCodeResultModel>()
            .ForMember(x => x.TimeCodeType, p => p.MapFrom(o => o.VideoTimeCode != null ? o.VideoTimeCode.TimeCodeType : default));
        }
    }
}
