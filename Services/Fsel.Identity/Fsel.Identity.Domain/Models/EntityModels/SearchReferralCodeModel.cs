// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class SearchReferralCodeModel
    {
        public Guid SenderId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? Code { get; set; }
        public string? Link { get; set; }
        public int NumberUser { get; set; }
        public int TotalToken { get; set; }
    }
}
