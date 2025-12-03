// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.VideoResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.V1i2;

    public class VideoResultProfile : Profile
    {
        public VideoResultProfile()
        {
            CreateMap<VideoResult, VideoResultModel>().IgnoreAllNonExisting();
            CreateMap<ReviewLessonVideoCommandModel, VideoResult>().IgnoreAllNonExisting();

            CreateMap<VideoResult, ResultModel>().IgnoreAllNonExisting();
        }
    }
}
