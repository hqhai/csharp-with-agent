// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ErrorReports
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Domain.Enums;

    public class UpdateErrorReportCommandModel : BaseCommandModel
    {
        public EnumTypeOfError TypeOfError { get; set; }

        public EnumPriority Priority { get; set; }

        public EnumErrorReportStatus ReportStatus { get; set; }

        public string? Url { get; set; }

        public string? FselFeedBack { get; set; }

        public IList<string>? ImageLinks { get; set; }

        public string? StudentFeedBack { get; set; }
    }
}
