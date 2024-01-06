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
            CreateMap<VideoTimeCodeResult, TestResultRankingModel>()
            .ForMember(x => x.Score, p => p.MapFrom(o => o.CorrectCount));
            CreateMap<VideoTimeCodeResult, TestResultReportModel>()
            .ForMember(x => x.Score, p => p.MapFrom(o => o.CorrectCount));
            CreateMap<VideoTimeCodeResult, VideoTimeCodeResultModel>()
            .ForMember(x => x.CurrentVideoTimeCodeId, p => p.MapFrom(o => o.VideoResult != null ? o.VideoResult.CurrentVideoTimeCodeId : null));
        }
    }
}
