using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Common.Models.Commands.HomeWork;
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
    public class HomeWorkProfile : Profile
    {
        public HomeWorkProfile()
        {
            CreateMap<HomeWork, HomeWorkModel>().IgnoreAllNonExisting();
            CreateMap<CreateHomeWorkCommandModel, HomeWork>().IgnoreAllNonExisting();
            CreateMap<UpdateHomeWorkCommandModel, HomeWork>().IgnoreAllNonExisting();
        }
    }
}