// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Models.CommandModels.Comments;
    using Fsel.Interaction.Domain.Models.EntityModels;

    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<Comments, CommentModel>().IgnoreAllNonExisting();
            CreateMap<CreateCommentCommandModel, Comments>().IgnoreAllNonExisting();
        }
    }
}
