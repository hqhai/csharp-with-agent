using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Common.Models.Commands.Videos;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;

namespace Fsel.Course.Infrastructure.Maps
{
    public class VideoProfile : Profile
    {
        public VideoProfile()
        {
            CreateMap<Video, VideoModel>().IgnoreAllNonExisting();
            CreateMap<CreateVideoCommandModel, Video>().IgnoreAllNonExisting();
            CreateMap<UpdateVideoCommandModel, Video>().IgnoreAllNonExisting();
        }
    }
}