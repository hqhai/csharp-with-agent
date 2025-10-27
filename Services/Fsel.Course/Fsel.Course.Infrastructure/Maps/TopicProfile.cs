// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.Topics;
    using Fsel.Course.Domain.Models.EntityModels;

    public class TopicProfile : Profile
    {
        public TopicProfile()
        {
            CreateMap<Topic, TopicModel>().IgnoreAllNonExisting();
            CreateMap<CreateTopicCommandModel, Topic>().IgnoreAllNonExisting();
            CreateMap<UpdateTopicCommandModel, Topic>().IgnoreAllNonExisting();
        }
    }
}
