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

            CreateMap<Student, StudentModel>()
                .ForMember(p => p.ParentEmail, n => n.MapFrom(m => m.ParentStudents.Count > 0 ? m.ParentStudents.FirstOrDefault()!.Parent!.User!.Email : null))
                .ForMember(x => x.SenderId, v => v.MapFrom(b => (b.User != null && b.User.Receiver != null) ? (Guid?)b.User.Receiver.SenderId : null))
                .ForMember(x => x.EmailParent, v => v.MapFrom(b => b.ParentEmail));

            CreateMap<CreateStudentCommandModel, Student>().IgnoreAllNonExisting();
            CreateMap<UpdateStudentBeginnerGuideCommandModel, Student>().IgnoreAllNonExisting();
            CreateMap<CreateStudentByParentCommandModel, Student>().IgnoreAllNonExisting();
            CreateMap<UpdateUserProfileCommandModel, Student>().IgnoreAllNonExisting();
            CreateMap<UpdateStudentProfileCommandModel, Student>().IgnoreAllNonExisting();
            CreateMap<UpdateStudentByAdminCommandModel, Student>().ForMember(m => m.Id, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<Student, UserProfileModel>().ForMember(m => m.Id, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<Student, StudentProfileModel>().IgnoreAllNonExisting();
            CreateMap<Student, StudentDetailModel>().IgnoreAllNonExisting();
        }
    }
}