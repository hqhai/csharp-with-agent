// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;

    public class VideoTimeCodeAnswerProfile : Profile
    {
        public VideoTimeCodeAnswerProfile()
        {
            CreateMap<VideoTimeCodeAnswer, VideoTimeCodeAnswerModel>().IgnoreAllNonExisting();
        }
    }
}
