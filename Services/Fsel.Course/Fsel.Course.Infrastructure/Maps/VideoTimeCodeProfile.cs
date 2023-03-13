using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodes;
using Fsel.Course.Domain.Models.EntiyModels;

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
