using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Common.Models.Commands.ExtraPractice;
using Fsel.Course.Common.Models.Commands.Lesson;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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