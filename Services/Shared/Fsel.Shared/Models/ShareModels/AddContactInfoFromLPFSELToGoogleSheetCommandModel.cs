// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    public class AddContactInfoFromLPFSELToGoogleSheetCommandModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime BirthDay { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? School { get; set; }
        public string? Cohort { get; set; }
        public string? Class { get; set; }
        public string? StudentCode { get; set; }
    }
}
