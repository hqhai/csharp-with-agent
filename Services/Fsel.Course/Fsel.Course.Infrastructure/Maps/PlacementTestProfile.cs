using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Common.Models.Commands.PlacementTest;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;

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