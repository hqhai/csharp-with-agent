using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Common.Models.Commands.ExtraPractice;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;

namespace Fsel.Course.Infrastructure.Maps
{
    public class ExtraPracticeProfile : Profile
    {
        public ExtraPracticeProfile()
        {
            CreateMap<ExtraPractice, ExtraPracticeModel>().IgnoreAllNonExisting();
            CreateMap<CreateExtraPracticeCommandModel, ExtraPractice>().IgnoreAllNonExisting();
            CreateMap<UpdateExtraPracticeCommandModel, ExtraPractice>().IgnoreAllNonExisting();
        }
    }
}