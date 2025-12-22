// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.GoogleSheets
{
    using System.Collections.Generic;

    public class CreateDynamicInfosToGoogleSheetFileCommandModel
    {
        public IList<Dictionary<string, object>>? Model { get; set; }
        public string? OverrideSpreadSheetId { get; set; }
        public string? OverrideSheet { get; set; }
        public IList<string>? ColumnOrder { get; set; }
    }
}
