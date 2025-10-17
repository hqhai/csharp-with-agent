// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.Videos;
using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Infrastructure.Maps
{
    public class VideoProfile : Profile
    {
        public VideoProfile()
        {
            CreateMap<Video, VideoModel>().IgnoreAllNonExisting();
            CreateMap<CreateVideoCommandModel, Video>().IgnoreAllNonExisting();
            CreateMap<UpdateVideoCommandModel, Video>().IgnoreAllNonExisting();
            CreateMap<CreateVideoSubFilePathCommandModel, VideoSubFilePath>().IgnoreAllNonExisting();
            CreateMap<VideoSubFilePath, VideoSubFilePathModel>().IgnoreAllNonExisting();
        }
    }
}
