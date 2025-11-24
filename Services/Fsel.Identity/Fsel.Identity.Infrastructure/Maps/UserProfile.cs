// Copyright (c) Atlantic. All rights reserved.

using System.Globalization;
using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Models.CommandModels.Admins;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using Fsel.Identity.Domain.Models.CommandModels.Parents;
using Fsel.Identity.Domain.Models.CommandModels.OpenId;
using Fsel.Identity.Domain.Models.CommandModels.Students;
using Fsel.Identity.Domain.Models.CommandModels.Users;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Shared.Helpers;

namespace Fsel.Identity.Infrastructure.Maps
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserModel>().IgnoreAllNonExisting();
            CreateMap<User, UserProfileModel>().IgnoreAllNonExisting();
            CreateMap<CreateStudentByParentCommandModel, User>()
                .AfterMap<ParseFullNameMappingAction<CreateStudentByParentCommandModel>>()
                .IgnoreAllNonExisting();
            CreateMap<CreateUserCommandModel, User>()
                .AfterMap<ParseFullNameMappingAction<CreateUserCommandModel>>()
                .IgnoreAllNonExisting();
            CreateMap<UpdateUserCommandModel, User>()
                .AfterMap<ParseFullNameMappingAction<UpdateUserCommandModel>>()
                .IgnoreAllNonExisting();
            CreateMap<UpdateParentCommandModel, User>()
                .AfterMap<ParseFullNameMappingAction<UpdateParentCommandModel>>()
                .IgnoreAllNonExisting();
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
                .ForMember(m => m.Id, opt => opt.Ignore())
                .AfterMap<ParseFullNameMappingAction<UpdateStudentByAdminCommandModel>>();

            CreateMap<User, StudentModel>().ForMember(m => m.Id, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<SignUpCommandModel, User>()
                .AfterMap<ParseFullNameMappingAction<SignUpCommandModel>>()
                .IgnoreAllNonExisting();
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
                .ForMember(m => m.Id, opt => opt.Ignore())
                .AfterMap<ParseFullNameMappingAction<UpdateUserProfileCommandModel>>();

            CreateMap<UpdateCodeStudentCommandModel, User>().AfterMap<ParseFullNameMappingAction<UpdateCodeStudentCommandModel>>().IgnoreAllNonExisting();

            CreateMap<UpdateStudentProfileCommandModel, User>()
                .AfterMap<ParseFullNameMappingAction<UpdateStudentProfileCommandModel>>()
                .IgnoreAllNonExisting();
            CreateMap<UpdateProfileStudentCommandModel, User>()
                .AfterMap<ParseFullNameMappingAction<UpdateProfileStudentCommandModel>>()
                .IgnoreAllNonExisting();
            CreateMap<UserRegisterModel, User>()
                .AfterMap<ParseFullNameMappingAction<UserRegisterModel>>()
                .IgnoreAllNonExisting();

            CreateMap<UpdateStudentProfileCommandModel, User>().AfterMap<ParseFullNameMappingAction<UpdateStudentProfileCommandModel>>().IgnoreAllNonExisting();
            CreateMap<UpdateProfileStudentCommandModel, User>().AfterMap<ParseFullNameMappingAction<UpdateProfileStudentCommandModel>>().IgnoreAllNonExisting();
            CreateMap<GetAccountDashboardQueryModel, ExportAccountDashboardCommandModel>().IgnoreAllNonExisting();
            CreateMap<CreateUserToLmsAdminPlatCommandModel, User>().AfterMap<ParseFullNameMappingAction<CreateUserToLmsAdminPlatCommandModel>>().IgnoreAllNonExisting();
            CreateMap<UpdateUserInLmsAdminPlatCommandModel, User>().AfterMap<ParseFullNameMappingAction<UpdateUserInLmsAdminPlatCommandModel>>().IgnoreAllNonExisting();
            CreateMap<ParentProfileModel, User>()
                .AfterMap<ParseFullNameMappingAction<ParentProfileModel>>()
                .IgnoreAllNonExisting();
        }

        public class ParseFullNameMappingAction<TSource> : IMappingAction<TSource, User>
            where TSource : class
        {
            public void Process(TSource source, User? destination, ResolutionContext context)
            {
                // Kiểm tra nếu source có FullName
                var fullNameProp = typeof(TSource).GetProperty(nameof(User.FullName));
                if (destination != null && fullNameProp != null)
                {
                    var fullNameValue = fullNameProp.GetValue(source) as string;
                    if (!string.IsNullOrEmpty(fullNameValue))
                    {
                        var parsed = fullNameValue.ParseFullName();
                        destination.FirstName = parsed.FirstName;
                        destination.LastName = parsed.LastName;
                    }
                }
            }
        }
    }
}
