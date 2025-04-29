// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.Auths;
    using Fsel.Identity.Domain.Models.CommandModels.Humans;
    using Fsel.Identity.Domain.Models.CommandModels.Parents;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class HumanProfile : Profile
    {
        public HumanProfile()
        {
            CreateMap<Human, HumanModel>().IgnoreAllNonExisting();
            CreateMap<CreateHumanCommandModel, Human>().IgnoreAllNonExisting();
            CreateMap<UpdateHumanCommandModel, Human>().IgnoreAllNonExisting();
            CreateMap<SignUpCommandModel, Human>().IgnoreAllNonExisting();
            CreateMap<User, Human>().IgnoreAllNonExisting();
            CreateMap<TokenModel, ConfirmOtpModel>().IgnoreAllNonExisting();
            CreateMap<CreateStudentByParentCommandModel, Human>().IgnoreAllNonExisting();
            CreateMap<CreateUserCommandModel, Human>().IgnoreAllNonExisting();
            CreateMap<UpdateUserCommandModel, Human>().ForMember(m => m.Id, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<UpdateStudentByAdminCommandModel, Human>().ForMember(m => m.Id, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<UpdateUserProfileCommandModel, Human>().IgnoreAllNonExisting();
            CreateMap<UpdateParentCommandModel, Human>().IgnoreAllNonExisting();
            CreateMap<UpdateStudentProfileCommandModel, Human>().IgnoreAllNonExisting();
            CreateMap<Human, UserProfileModel>().ForMember(m => m.Id, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<Human, HumanProfileModel>().IgnoreAllNonExisting();
            CreateMap<UpdateCodeStudentCommandModel, Human>().IgnoreAllNonExisting();
            CreateMap<Human, ParentInfoModel>().IgnoreAllNonExisting();
            CreateMap<Human, StudentModel>().IgnoreAllNonExisting();
            CreateMap<Human, ParentProfileModel>().IgnoreAllNonExisting();
            CreateMap<UpdateProfileStudentCommandModel, Human>().IgnoreAllNonExisting();
            CreateMap<User, UserInformationModel>().IgnoreAllNonExisting();
            CreateMap<UpdateProfileUserCommandModel, User>().IgnoreAllNonExisting();
            CreateMap<UpdateProfileUserCommandModel, Human>().ForMember(x => x.Id, c => c.Ignore()).IgnoreAllNonExisting();
            CreateMap<CreateUserToLmsAdminPlatCommandModel, Human>().IgnoreAllNonExisting();
            CreateMap<UpdateUserInLmsAdminPlatCommandModel, Human>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .IgnoreAllNonExisting();
        }
    }
}
