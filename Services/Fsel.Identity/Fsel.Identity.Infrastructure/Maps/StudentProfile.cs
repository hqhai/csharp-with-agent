// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Domain.Models.CommandModels.Humans;
    using Fsel.Identity.Domain.Models.CommandModels.Students;

    public class StudentProfile : Profile
    {
        public StudentProfile()
        {
            CreateMap<Student, StudentModel>().IgnoreAllNonExisting();
            CreateMap<CreateStudentCommandModel, Student>().IgnoreAllNonExisting();
            CreateMap<CreateStudentCommandModel, Student>().IgnoreAllNonExisting();
        }
    }
}
