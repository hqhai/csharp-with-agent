// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.ErrorReports;
    using Fsel.System.Domain.Models.EntityModels;

    public class ErrorReportConfigProfile : Profile
    {
        public ErrorReportConfigProfile()
        {
            CreateMap<ErrorReport, ErrorReportModel>().IgnoreAllNonExisting();
            CreateMap<CreateErrorReportCommandModel, ErrorReport>().IgnoreAllNonExisting();
            CreateMap<UpdateErrorReportCommandModel, ErrorReport>().IgnoreAllNonExisting();
            CreateMap<UpdateErrorReportPriorityCommandModel, ErrorReport>().IgnoreAllNonExisting();
            CreateMap<UpdateErrorReportStatusCommandModel, ErrorReport>().IgnoreAllNonExisting();
            CreateMap<UpdateErrorReportTypeErrorCommandModel, ErrorReport>().IgnoreAllNonExisting();
        }
    }
}
