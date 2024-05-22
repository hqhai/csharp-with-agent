// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    using System.Collections.Generic;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Shared.Enums;

    public class CreateOrdersFromCRMCommandModels
    {
        public IList<CreateOrdersFromCRMCommandModel>? UsersInfo { get; set; }
    }

    public class CreateOrdersFromCRMCommandModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public EnumGender? Gender { get; set; }
        public DateTime Birthday { get; set; }
        public string? MotherName { get; set; }
        public string? MotherEmail { get; set; }
        public string? MotherPhoneNumber { get; set; }
        public string? FatherName { get; set; }
        public string? FatherEmail { get; set; }
        public string? FatherPhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? LongPath { get; set; }
        public int? DiscountPercent { get; set; }
        public int Package { get; set; }
    }
}
