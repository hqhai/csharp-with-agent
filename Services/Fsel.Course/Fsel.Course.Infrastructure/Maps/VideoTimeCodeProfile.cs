using AutoMapper;
using Fsel.Course.Common.Models.Commands.Videos;
using Fsel.Course.Common.Models.Commands.VideoTimeCode;
using Fsel.Course.Common.Models.Entities;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Infrastructure.Maps
{
    public class VideoTimeCodeProfile : Profile
    {
        public VideoTimeCodeProfile()
        {
            CreateMap<VideoTimeCode, VideoTimeCodeModel>().IgnoreAllNonExisting();
            CreateMap<CreateVideoTimeCodeCommandModel, VideoTimeCode>().IgnoreAllNonExisting();
            CreateMap<UpdateVideoTimeCodeCommandModel, VideoTimeCode>().IgnoreAllNonExisting();
        }
    }
}