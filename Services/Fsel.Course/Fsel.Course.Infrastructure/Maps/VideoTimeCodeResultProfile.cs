// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.VideoTimeCodeResults;

    public class VideoTimeCodeResultProfile : Profile
    {
        public VideoTimeCodeResultProfile()
        {
            CreateMap<VideoTimeCodeResult, VideoTimeCodeResultModel>().IgnoreAllNonExisting();
            CreateMap<VideoTimeCodeResult, VideoTimeCodeResultByStudentModel>().IgnoreAllNonExisting();
        }
    }
}
