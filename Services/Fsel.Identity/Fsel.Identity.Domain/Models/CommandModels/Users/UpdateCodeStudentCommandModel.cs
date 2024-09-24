// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Users
{
    using System;
    using Fsel.Shared.Enums;

    public class UpdateCodeStudentCommandModel
    {
        public Guid? UserId { get; set; }
        public EnumGender Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public int? YearBirthday { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SchoolId { get; set; }
        public string? SchoolName { get; set; }
    }
}
