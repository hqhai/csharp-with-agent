// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Models.CommandModels.UserGroup;

namespace Fsel.Identity.Infrastructure.Maps
{
    public class UserGroupCommandProfile : Profile
    {
        public UserGroupCommandProfile()
        {
            // Mapping cho CreateUserGroup và UpdateUserGroup
            CreateMap<CreateUserGroupCommandModel, UserGroup>().IgnoreAllNonExisting();

            // Mapping cho AddUserToGroup
            CreateMap<AddUserToGroupCommandModel, UserGroupMemberShip>().IgnoreAllNonExisting();

            // Mapping cho RemoveUserFromGroup
            CreateMap<RemoveUserFromGroupCommandModel, UserGroupMemberShip>().IgnoreAllNonExisting();
        }
    }
}
