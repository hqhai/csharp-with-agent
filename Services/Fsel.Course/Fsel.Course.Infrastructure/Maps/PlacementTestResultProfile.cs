// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.PlacementTestResults;
    using Fsel.Course.Domain.Models.EntityModels;

    public class PlacementTestResultProfile : Profile
    {
        public PlacementTestResultProfile()
        {
            CreateMap<PlacementTestResult, PlacementTestResultModel>().IgnoreAllNonExisting();
            CreateMap<UpdatePlacementTestResultCommandModel, PlacementTestResult>().IgnoreAllNonExisting();
        }
    }
}
