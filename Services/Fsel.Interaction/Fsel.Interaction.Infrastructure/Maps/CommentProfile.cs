// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Models.CommandModels.Comments;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Models.ShareModels;

    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<Comment, CommentModel>().IgnoreAllNonExisting();
            CreateMap<Comment, CommentQueueModel>().IgnoreAllNonExisting();
            CreateMap<CreateCommentCommandModel, Comment>().IgnoreAllNonExisting();
        }
    }
}
