// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;

    public class RegisterStudentForEventCommandModel
    {
        public string? EventCode { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime BirthDay { get; set; }
        public string? Province { get; set; }
        public Guid ProvinceId { get; set; }
        public string? District { get; set; }
        public Guid DistrictId { get; set; }
        public string? School { get; set; }
        public Guid SchoolId { get; set; }
        public string? SchoolGrade { get; set; }
        public string? SchoolClass { get; set; }
        public string? SchoolStudentCode { get; set; }
    }
}
