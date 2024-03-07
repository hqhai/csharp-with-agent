// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ErrorReports
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Domain.Enums;

    public class UpdateErrorReportTypeErrorCommandModel : BaseCommandModel
    {
        public EnumTypeOfError Type { get; set; }
    }
}
