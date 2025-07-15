// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Models.EntityModels;

namespace Fsel.Identity.Infrastructure.Maps
{
    public class UserGroupProfile : Profile
    {
        public UserGroupProfile()
        {
            // Map từ Role sang UserGroupModel
            CreateMap<Role, UserGroupModel>()
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            // Map từ UserGroupModel sang Role
            CreateMap<UserGroupModel, Role>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.GroupName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
        }
    }
} 