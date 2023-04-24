// Copyright (c) Atlantic. All rights reserved.
using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.Users;
using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Infrastructure.Maps
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserModel>().IgnoreAllNonExisting();
            CreateMap<CreateUserCommandModel, User>().IgnoreAllNonExisting();
            CreateMap<UpdateUserCommandModel, User>().IgnoreAllNonExisting();
        }
    }
}
