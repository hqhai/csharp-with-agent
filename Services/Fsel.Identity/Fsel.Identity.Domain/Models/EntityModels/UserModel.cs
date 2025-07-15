// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class UserModel : BaseModel
    {
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Address { get; set; }
        public string? School { get; set; }
        public string? SchoolName { get; set; }
        public Guid? UserGroupId { get; set; }
        public string? UserGroupName { get; set; }
        public HumanModel? Human { get; set; }
    }
}
