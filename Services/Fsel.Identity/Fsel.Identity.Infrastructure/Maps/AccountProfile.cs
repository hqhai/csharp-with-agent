// Copyright (c) Atlantic. All rights reserved.

using System.Globalization;
using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Models.CommandModels.Admins;
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
                .BeforeMap((m, c) =>
                {
                    c.UserName = (c.Email?.ToLower(CultureInfo.CurrentCulture).Trim() == c.UserName?.ToLower(CultureInfo.CurrentCulture).Trim()) ? m.Email
                                : (c.PhoneNumber?.Trim() == c.UserName?.Trim()) ? m.PhoneNumber
                                : c.UserName;
                    c.NormalizedUserName = (c.Email?.ToLower(CultureInfo.CurrentCulture).Trim() == c.UserName?.ToLower(CultureInfo.CurrentCulture).Trim()) ? m.Email?.ToUpper(CultureInfo.CurrentCulture)
                                           : (c.PhoneNumber?.Trim() == c.UserName?.Trim()) ? m.PhoneNumber
                                           : c.NormalizedUserName;
                })
                .ForMember(p => p.NormalizedEmail, n => n.MapFrom(m => (m.Email ?? string.Empty).ToUpper(CultureInfo.CurrentCulture)))
                .ForMember(m => m.Id, opt => opt.Ignore());

            CreateMap<User, StudentModel>().ForMember(m => m.Id, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<SignUpCommandModel, User>().IgnoreAllNonExisting();
            CreateMap<UpdateUserProfileCommandModel, User>()
                .BeforeMap((m, c) =>
                {
                    c.UserName = (c.Email?.ToLower(CultureInfo.CurrentCulture).Trim() == c.UserName?.ToLower(CultureInfo.CurrentCulture).Trim()) ? c.Email
                    : (c.PhoneNumber?.Trim() == c.UserName?.Trim()) ? m.PhoneNumber
                    : c.UserName;
                    c.NormalizedUserName = (c.Email?.ToLower(CultureInfo.CurrentCulture).Trim() == c.UserName?.ToLower(CultureInfo.CurrentCulture).Trim()) ? (c.Email ?? string.Empty)?.ToUpper(CultureInfo.CurrentCulture)
                    : (c.PhoneNumber?.Trim() == c.UserName?.Trim()) ? m.PhoneNumber
                    : c.NormalizedUserName;
                })
                .ForMember(m => m.Id, opt => opt.Ignore());

            CreateMap<UpdateStudentProfileCommandModel, User>().IgnoreAllNonExisting();
            CreateMap<UpdateProfileStudentCommandModel, User>().IgnoreAllNonExisting();
            CreateMap<GetAccountDashboardQueryModel, ExportAccountDashboardCommandModel>().IgnoreAllNonExisting();
            CreateMap<CreateUserToLmsAdminPlatCommandModel, User>().IgnoreAllNonExisting();
            CreateMap<UpdateUserInLmsAdminPlatCommandModel, User>().IgnoreAllNonExisting();
            CreateMap<SignUpSMSCommandModel, User>().IgnoreAllNonExisting();
        }
    }
}
