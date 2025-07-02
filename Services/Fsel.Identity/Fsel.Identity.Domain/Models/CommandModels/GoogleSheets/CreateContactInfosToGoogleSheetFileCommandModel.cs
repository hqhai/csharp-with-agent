// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.GoogleSheets
{
    public class CreateContactInfoToGoogleSheetFileCommandModel
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
    }
    public class CreateContactInfosToGoogleSheetFileCommandModel
    {
        public IList<CreateContactInfoToGoogleSheetFileCommandModel>? Model { get; set; }
        public string? OverrideSpreadSheetId { get; set; }
        public string? OverrideSheet { get; set; }
        public IList<string>? ColumnOrder { get; set; }
    }
}
