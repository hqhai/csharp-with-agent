// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class TeacherProfile : Profile
    {
        public TeacherProfile()
        {
            CreateMap<Teacher, TeacherModel>().IgnoreAllNonExisting();
            CreateMap<CreateUserCommandModel, Teacher>().IgnoreAllNonExisting();
            CreateMap<UpdateUserCommandModel, Teacher>().ForMember(m => m.Id, opt => opt.Ignore()).IgnoreAllNonExisting();

            CreateMap<UpdateUserProfileCommandModel, Teacher>().IgnoreAllNonExisting();
        }
    }
}
