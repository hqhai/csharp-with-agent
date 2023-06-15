// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models;
    using Fsel.System.Domain.Models.CommandModels.LiveTimeFrames;

    public class LiveTimeFrameProfile : Profile
    {
        public LiveTimeFrameProfile()
        {
            CreateMap<LiveTimeFrame, LiveTimeFrameModel>().IgnoreAllNonExisting();
            CreateMap<SaveListLiveTimeFrameCommandModel, LiveTimeFrame>().IgnoreAllNonExisting();
            CreateMap<SaveLiveTimeFrameCommandModel, LiveTimeFrame>().IgnoreAllNonExisting();
        }
    }
}
