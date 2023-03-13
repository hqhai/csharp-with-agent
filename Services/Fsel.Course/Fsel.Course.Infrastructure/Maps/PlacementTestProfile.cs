using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.PlacementTests;
using Fsel.Course.Domain.Models.EntiyModels;

namespace Fsel.Course.Infrastructure.Maps
{
    public class PlacementTestProfile : Profile
    {
        public PlacementTestProfile()
        {
            CreateMap<PlacementTest, PlacementTestModel>().IgnoreAllNonExisting();
            CreateMap<CreatePlacementTestCommandModel, PlacementTest>().IgnoreAllNonExisting();
            CreateMap<UpdatePlacementTestCommandModel, PlacementTest>().IgnoreAllNonExisting();
        }
    }
}
