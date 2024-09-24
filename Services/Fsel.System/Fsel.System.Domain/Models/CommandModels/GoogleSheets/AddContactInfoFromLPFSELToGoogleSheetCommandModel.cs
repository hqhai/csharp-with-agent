// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.GoogleSheets
{
    public class AddContactInfoFromLPFSELToGoogleSheetCommandModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateTime Birthday { get; set; }
    }
}
