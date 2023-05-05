// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.Parents;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class StudentProfile : Profile
    {
        public StudentProfile()
        {
            CreateMap<Student, StudentModel>().IgnoreAllNonExisting();
            CreateMap<CreateStudentCommandModel, Student>().IgnoreAllNonExisting();
            CreateMap<CreateStudentByParentCommandModel, Student>().IgnoreAllNonExisting();
            CreateMap<UpdateUserProfileCommandModel, Student>().IgnoreAllNonExisting();
            CreateMap<UpdateStudentProfileCommandModel, Student>().IgnoreAllNonExisting();
        }
    }
}
