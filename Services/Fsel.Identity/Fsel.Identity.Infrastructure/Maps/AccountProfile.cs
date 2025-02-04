// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using Fsel.Identity.Domain.Models.CommandModels.Parents;
using Fsel.Identity.Domain.Models.CommandModels.Students;
using Fsel.Identity.Domain.Models.CommandModels.Users;
using Fsel.Identity.Domain.Models.EntityModels;

namespace Fsel.Identity.Infrastructure.Maps
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserModel>().IgnoreAllNonExisting();
            CreateMap<User, UserProfileModel>().IgnoreAllNonExisting();
            CreateMap<CreateStudentByParentCommandModel, User>().IgnoreAllNonExisting();
            CreateMap<CreateUserCommandModel, User>().IgnoreAllNonExisting();
            CreateMap<UpdateUserCommandModel, User>().IgnoreAllNonExisting();
            CreateMap<UpdateStudentByAdminCommandModel, User>()
                .ForMember(p => p.UserName, n => n.MapFrom((m, p) => p.UserName.ToLower().Trim() == p.Email.ToLower().Trim() ? m.Email : m.PhoneNumber))
                .ForMember(p => p.Email, n => n.MapFrom(m => m.Email))
                .ForMember(p => p.PhoneNumber, n => n.MapFrom(m => m.PhoneNumber))
                .ForMember(p => p.NormalizedUserName, n => n.MapFrom((m, p) => p.UserName.ToLower().Trim() == p.Email.ToLower().Trim() ? m.Email.ToLower() : m.PhoneNumber.ToLower()))
                .ForMember(p => p.NormalizedEmail, n => n.MapFrom(m => m.Email.ToLower()))
                .ForMember(m => m.Id, opt => opt.Ignore());

            CreateMap<User, StudentModel>().ForMember(m => m.Id, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<SignUpCommandModel, User>().IgnoreAllNonExisting();
            CreateMap<UpdateUserProfileCommandModel, User>().IgnoreAllNonExisting();
            CreateMap<UpdateStudentProfileCommandModel, User>().IgnoreAllNonExisting();
            CreateMap<UpdateProfileStudentCommandModel, User>().IgnoreAllNonExisting();
        }
    }
}
