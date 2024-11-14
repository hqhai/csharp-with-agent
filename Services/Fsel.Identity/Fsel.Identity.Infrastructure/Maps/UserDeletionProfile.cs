// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.UserDeletions;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class UserDeletionProfile : Profile
    {
        public UserDeletionProfile()
        {
            CreateMap<UserDeletion, UserDeletionModel>().IgnoreAllNonExisting();
            CreateMap<CreateUserDeletionCommandModel, UserDeletion>().IgnoreAllNonExisting();
        }
    }
}
