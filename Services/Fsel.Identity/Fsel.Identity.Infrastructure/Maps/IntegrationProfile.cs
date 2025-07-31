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
                CreateMap<Human, ClientsIntegrationModel>()
                        .ForMember(x => x.UserId, a => a.MapFrom(src => src.UserId))
                        .ForMember(x => x.FullName, a => a.MapFrom(src => src.User != null ? src.User.FullName : null))
                        .ForMember(x => x.UserName, a => a.MapFrom(src => src.User != null ? src.User.UserName : null))
                        .ForMember(x => x.CreateAccount, a => a.MapFrom(src => src.User != null ? src.User.CreatedDate : (DateTime?)null))
                        .ForMember(x => x.SchoolId, a => a.MapFrom(src => src.Student != null ? src.Student.SchoolId : null))
                        .ForMember(x => x.StudentEmail, a => a.MapFrom(src => src.Email))
                        .ForMember(x => x.StudentPhone, a => a.MapFrom(src => src.PhoneNumber))
                        .ForMember(x => x.HumanCode, a => a.MapFrom(src => src.Code))
                        .ForMember(x => x.CourseLevel, a => a.MapFrom(src => src.Student != null ? src.Student.CourseLevel : null))
                        .ForMember(x => x.SchoolGrade, a => a.MapFrom(src => src.Student != null ? src.Student.SchoolGrade : null))
                        .ForMember(x => x.SchoolClass, a => a.MapFrom(src => src.Student != null ? src.Student.SchoolClass : null))
                        .ForMember(x => x.ExpireDate, a => a.MapFrom(src => src.Student != null ? src.Student.ExpiredDate : null))
                        .ForMember(x => x.ParentName, a => a.MapFrom(src => src.Student.ParentStudents.FirstOrDefault().Parent.Human.FullName))
                        .ForMember(x => x.ParentPhone, a => a.MapFrom(src => src.Student.ParentStudents.FirstOrDefault().Parent.Human.PhoneNumber))
                        .ForMember(x => x.ParentEmail, a => a.MapFrom(src => src.Student.ParentStudents.FirstOrDefault().Parent.Human.Email))
                        .ForMember(x => x.ParentGender, a => a.MapFrom(src => src.Student.ParentStudents.FirstOrDefault().Parent.Human.Gender));

                CreateMap<Human, LeadsIntegrationModel>()
                        .ForMember(x => x.UserId, a => a.MapFrom(src => src.UserId))
                        .ForMember(x => x.FullName, a => a.MapFrom(src => src.User != null ? src.User.FullName : null))
                        .ForMember(x => x.UserName, a => a.MapFrom(src => src.User != null ? src.User.UserName : null))
                        .ForMember(x => x.CreateAccount, a => a.MapFrom(src => src.User != null ? src.User.CreatedDate : (DateTime?)null))
                        .ForMember(x => x.SchoolId, a => a.MapFrom(src => src.Student != null ? src.Student.SchoolId : null))
                        .ForMember(x => x.StudentEmail, a => a.MapFrom(src => src.Email))
                        .ForMember(x => x.StudentPhone, a => a.MapFrom(src => src.PhoneNumber))
                        .ForMember(x => x.CourseLevel, a => a.MapFrom(src => src.Student != null ? src.Student.CourseLevel : null))
                        .ForMember(x => x.ExpireDate, a => a.MapFrom(src => src.Student != null ? src.Student.ExpiredDate : null))
                        .ForMember(x => x.ParentName, a => a.MapFrom(src => src.Student.ParentStudents.FirstOrDefault().Parent.Human.FullName))
                        .ForMember(x => x.ParentPhone, a => a.MapFrom(src => src.Student.ParentStudents.FirstOrDefault().Parent.Human.PhoneNumber))
                        .ForMember(x => x.ParentEmail, a => a.MapFrom(src => src.Student.ParentStudents.FirstOrDefault().Parent.Human.Email))
                        .ForMember(x => x.ParentGender, a => a.MapFrom(src => src.Student.ParentStudents.FirstOrDefault().Parent.Human.Gender));
            }
        }
    }
}
