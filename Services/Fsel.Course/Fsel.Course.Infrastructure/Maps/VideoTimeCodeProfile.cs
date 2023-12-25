// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodes;
using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Infrastructure.Maps
{
    public class VideoTimeCodeProfile : Profile
    {
        public VideoTimeCodeProfile()
        {
            CreateMap<VideoTimeCode, VideoTimeCodeModel>().IgnoreAllNonExisting();
            CreateMap<CreateVideoTimeCodeCommandModel, VideoTimeCode>().IgnoreAllNonExisting();
            CreateMap<VideoTimeCodeResult, VideoTimeCodeResultModel>()
            .ForMember(x => x.CurrentVideoTimeCodeId, p => p.MapFrom(o => o.VideoResult != null ? o.VideoResult.CurrentVideoTimeCodeId : null));
        }
    }
}
