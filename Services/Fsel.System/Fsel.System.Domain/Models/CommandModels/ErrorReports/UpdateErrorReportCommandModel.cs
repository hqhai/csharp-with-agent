// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ErrorReports
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Domain.Enums;

    public class UpdateErrorReportCommandModel : BaseCommandModel
    {
        public EnumTypeOfError? Type { get; set; }

        public EnumPriority? Priority { get; set; }

        public EnumErrorReportStatus Status { get; set; }

        public string? Url { get; set; }

        public string? FeedBack { get; set; }

        public IList<string>? ImagePaths { get; set; }

        public string? Content { get; set; }
    }
}
