using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Models.CommandModels.StudentEditHistory;
using Fsel.Identity.Domain.Models.EntityModels;

namespace Fsel.Identity.Infrastructure.Maps
{
    public class StudentEditHistoryProfile : Profile
    {
        public StudentEditHistoryProfile()
        {
            CreateMap<CreateStudentEditHistoryCommandModel, StudentEditHistory>().IgnoreAllNonExisting();
            CreateMap<StudentEditHistoryDetailModel, StudentEditHistoryDetail>().IgnoreAllNonExisting();
            CreateMap<StudentEditHistory, StudentEditHistoryModel>().IgnoreAllNonExisting();
            CreateMap<StudentEditHistoryDetail, StudentEditHistoryDetailModel>().IgnoreAllNonExisting();
        }
    }
}
