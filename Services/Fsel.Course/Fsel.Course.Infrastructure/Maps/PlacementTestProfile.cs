using Fsel.Core.Extensions;
using Fsel.Course.Common.Models.Commands.PlacementTest;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Infrastructure.Maps
{
    public class PlacementTestProfile : Profile
    {
        public PlacementTestProfile()
        {
            CreateMap<PlacementTest, PlacementTestModel>().IgnoreAllNonExisting();
            CreateMap<CreatePlacementTestCommandModel, PlacementTest>().IgnoreAllNonExisting();
        }
    }
}
