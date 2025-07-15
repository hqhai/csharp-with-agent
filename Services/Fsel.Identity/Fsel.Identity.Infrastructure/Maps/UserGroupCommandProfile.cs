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
            // Mapping cho CreateUserGroup
            CreateMap<CreateUserGroupCommandModel, Role>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.GroupName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            // Mapping cho AddUserToGroup
            CreateMap<AddUserToGroupCommandModel, UserRole>()
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.GroupId));

            // Mapping cho RemoveUserFromGroup
            CreateMap<RemoveUserFromGroupCommandModel, UserRole>()
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.GroupId));
        }
    }
}
