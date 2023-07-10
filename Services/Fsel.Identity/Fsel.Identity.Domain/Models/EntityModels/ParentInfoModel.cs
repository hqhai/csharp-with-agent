// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class ParentInfoModel
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Occupation { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
    }
}
