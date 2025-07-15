// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Models.EntityModels;

namespace Fsel.Identity.Infrastructure.Maps
{
    public class UserGroupMemberShipProfile : Profile
    {
        public UserGroupMemberShipProfile()
        {
            // Map từ UserRole sang UserGroupMemberShipModel
            CreateMap<UserRole, UserGroupMemberShipModel>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.RoleId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            // Map từ UserGroupMemberShipModel sang UserRole
            CreateMap<UserGroupMemberShipModel, UserRole>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.GroupId));
        }
    }
} 