// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Quickstarts
{
    using System.ComponentModel.DataAnnotations;
    using System.Security.Claims;
    using Fsel.Common.Attributes;
    using Fsel.Shared.Enums;

    public class ExternalLoginModel
    {
        [EmailValid(ErrorMessage = "i18n_Email_is_not_valid")]
        [Required(ErrorMessage = "i18n_Email_cannot_be_empty")]
        public string? Email { get; set; }

        [PhoneValid(ErrorMessage = "i18n_number_phone_invalid")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "i18n_First_name_cannot_be_empty")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "i18n_Last_name_cannot_be_empty")]
        public string? LastName { get; set; }

        public EnumGender? Gender { get; set; }

        public int? DayBirthday { get; set; }

        public int? MonthBirthday { get; set; }

        public int? YearBirthday { get; set; }

        [Required(ErrorMessage = "i18n_Birth_day_cannot_be_empty")]
        public DateTime? Birthday => DayBirthday.HasValue && MonthBirthday.HasValue && YearBirthday.HasValue ? new DateTime(YearBirthday.Value, MonthBirthday.Value, DayBirthday.Value) : null;

        public string? Provider { get; set; }

        public ClaimsPrincipal? Principal { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
