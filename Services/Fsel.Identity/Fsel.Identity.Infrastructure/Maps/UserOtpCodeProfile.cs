// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class UserOtpCodeProfile : Profile
    {
        public UserOtpCodeProfile()
        {
            CreateMap<UserOtpCode, UserOtpCodeModel>().IgnoreAllNonExisting();
        }
    }
}
