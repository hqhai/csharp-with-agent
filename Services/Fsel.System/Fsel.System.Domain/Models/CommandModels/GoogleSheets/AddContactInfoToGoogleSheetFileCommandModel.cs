// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.GoogleSheets
{
    public class AddContactInfoToGoogleSheetFileCommandModel
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
    }
}
