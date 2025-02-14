// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Models.EntityModels.ManagerReportModels;

    public class OtherProfile : Profile
    {
        public OtherProfile()
        {
            CreateMap<OverallReportStudentAssiduityModel, SearchReportStudentAssiduityModel>().IgnoreAllNonExisting();
            CreateMap<OverallFeatureAccessTimeModel, StudentAssiduityModel>().IgnoreAllNonExisting();
        }
    }
}
