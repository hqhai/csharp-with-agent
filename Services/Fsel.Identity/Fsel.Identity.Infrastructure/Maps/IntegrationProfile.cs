// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.EntityModels.IntegrationModel;

    public class IntegrationProfile : Profile
    {
        public IntegrationProfile()
        {
            {
                CreateMap<User, ClientsIntegrationModel>()
                        .ForMember(x => x.UserId, a => a.MapFrom(src => src.Id))
                        .ForMember(x => x.FullName, a => a.MapFrom(src => src.FullName))
                        .ForMember(x => x.UserName, a => a.MapFrom(src => src.UserName))
                        .ForMember(x => x.CreateAccount, a => a.MapFrom(src => src.CreatedDate))
                        .ForMember(x => x.SchoolId, a => a.MapFrom(src => src.Student != null ? src.Student.SchoolId : null))
                        .ForMember(x => x.StudentEmail, a => a.MapFrom(src => src.Email))
                        .ForMember(x => x.StudentPhone, a => a.MapFrom(src => src.PhoneNumber))
                        .ForMember(x => x.HumanCode, a => a.MapFrom(src => src.Code))
                        .ForMember(x => x.CourseLevel, a => a.MapFrom(src => src.Student != null ? src.Student.CourseLevel : null))
                        .ForMember(x => x.SchoolGrade, a => a.MapFrom(src => src.Student != null ? src.Student.SchoolGrade : null))
                        .ForMember(x => x.SchoolClass, a => a.MapFrom(src => src.Student != null ? src.Student.SchoolClass : null))
                        .ForMember(x => x.ExpireDate, a => a.MapFrom(src => src.Student != null ? src.Student.ExpiredDate : null))
                        .ForMember(x => x.ParentName, a => a.MapFrom(src => src.Student.ParentStudents.FirstOrDefault().Parent.User.FullName))
                        .ForMember(x => x.ParentPhone, a => a.MapFrom(src => src.Student.ParentStudents.FirstOrDefault().Parent.User.PhoneNumber))
                        .ForMember(x => x.ParentEmail, a => a.MapFrom(src => src.Student.ParentStudents.FirstOrDefault().Parent.User.Email))
                        .ForMember(x => x.ParentGender, a => a.MapFrom(src => src.Student.ParentStudents.FirstOrDefault().Parent.User.Gender));

                CreateMap<User, LeadsIntegrationModel>()
                        .ForMember(x => x.UserId, a => a.MapFrom(src => src.Id))
                        .ForMember(x => x.UserName, a => a.MapFrom(src => src.UserName))
                        .ForMember(x => x.CreateAccount, a => a.MapFrom(src => src.CreatedDate))
                        .ForMember(x => x.SchoolId, a => a.MapFrom(src => src.Student != null ? src.Student.SchoolId : null))
                        .ForMember(x => x.StudentEmail, a => a.MapFrom(src => src.Email))
                        .ForMember(x => x.StudentPhone, a => a.MapFrom(src => src.PhoneNumber))
                        .ForMember(x => x.CourseLevel, a => a.MapFrom(src => src.Student != null ? src.Student.CourseLevel : null))
                        .ForMember(x => x.ExpireDate, a => a.MapFrom(src => src.Student != null ? src.Student.ExpiredDate : null))
                        .ForMember(x => x.ParentName, a => a.MapFrom(src => src.Student.ParentStudents.FirstOrDefault().Parent.User.FullName))
                        .ForMember(x => x.ParentPhone, a => a.MapFrom(src => src.Student.ParentStudents.FirstOrDefault().Parent.User.PhoneNumber))
                        .ForMember(x => x.ParentEmail, a => a.MapFrom(src => src.Student.ParentStudents.FirstOrDefault().Parent.User.Email))
                        .ForMember(x => x.ParentGender, a => a.MapFrom(src => src.Student.ParentStudents.FirstOrDefault().Parent.User.Gender));
            }
        }
    }
}
