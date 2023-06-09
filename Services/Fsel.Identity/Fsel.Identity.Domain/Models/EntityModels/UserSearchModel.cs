// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class UserSearchModel
    {
        public string? Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int NumberClass { get; set; }
        public string? Role { get; set; }
        public bool Status { get; set; }
        public string? FullName { get; set; }
    }
}
