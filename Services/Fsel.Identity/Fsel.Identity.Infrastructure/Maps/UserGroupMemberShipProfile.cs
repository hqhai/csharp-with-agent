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
            CreateMap<UserGroupMemberShip, UserGroupMemberShipModel>().IgnoreAllNonExisting();
            CreateMap<UserGroupMemberShipModel, UserGroupMemberShip>().IgnoreAllNonExisting();
        }
    }
} 