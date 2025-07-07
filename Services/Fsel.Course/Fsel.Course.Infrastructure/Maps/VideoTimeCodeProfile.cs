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
            CreateMap<VideoTimeCode, VideoTimeCodeModel>()
            .ForMember(x => x.CourseSkills, p => p.MapFrom(o => o.TimeCodeExercises != null && o.TimeCodeExercises.Any() ? o.TimeCodeExercises.Select(x => x.Exercise!.CourseSkill).Distinct().ToList() : null));
            CreateMap<CreateVideoTimeCodeCommandModel, VideoTimeCode>().IgnoreAllNonExisting();
            CreateMap<UpdateVideoTimeCodeCommandModel, VideoTimeCode>().IgnoreAllNonExisting();
            CreateMap<UpdateVideoTimeCodeCommandModel, CreateVideoTimeCodeCommandModel>().ForMember(p => p.Id, x => x.Ignore()).IgnoreAllNonExisting();
        }
    }
}
