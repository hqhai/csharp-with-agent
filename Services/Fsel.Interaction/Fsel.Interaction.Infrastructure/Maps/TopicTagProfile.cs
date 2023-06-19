// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Models.CommandModels.TopicTags;
    using Fsel.Interaction.Domain.Models.EntityModels;

    public class TopicTagProfile : Profile
    {
        public TopicTagProfile()
        {
            CreateMap<TopicTag, TopicTagModel>().IgnoreAllNonExisting();
            CreateMap<SaveTopicTagCommandModel, TopicTag>().IgnoreAllNonExisting();
        }
    }
}
