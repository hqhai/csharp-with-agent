// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Quickstarts
{
    using System.ComponentModel.DataAnnotations;
    using System.Security.Claims;
    using Fsel.Common.Attributes;
    using Fsel.Shared.Enums;

    public class ExternalLoginModel
    {
        [EmailValid(ErrorMessage = "Email is not valid.")]
        [Required(ErrorMessage = "Email cannot be empty.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "First name cannot be empty.")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last name cannot be empty.")]
        public string? LastName { get; set; }

        public EnumGender? Gender { get; set; }

        public int? DayBirthday { get; set; }

        public int? MonthBirthday { get; set; }

        public int? YearBirthday { get; set; }

        [Required(ErrorMessage = "Birth day cannot be empty.")]
        public DateTime? Birthday => DayBirthday.HasValue && MonthBirthday.HasValue && YearBirthday.HasValue ? new DateTime(YearBirthday.Value, MonthBirthday.Value, DayBirthday.Value) : null;

        public string? Provider { get; set; }

        public ClaimsPrincipal? Principal { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
