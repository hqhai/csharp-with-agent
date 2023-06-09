// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.TeacherBankAccount;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class TeacherBankAccountProfile : Profile
    {
        public TeacherBankAccountProfile()
        {
            CreateMap<TeacherBankAccount, TeacherBankAccountModel>().IgnoreAllNonExisting();
            CreateMap<UpdateUserCommandModel, TeacherBankAccount>().ForMember(m => m.Id, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<UpdateTeacherBankAccountCommandModel, TeacherBankAccount>().IgnoreAllNonExisting();
        }
    }
}
