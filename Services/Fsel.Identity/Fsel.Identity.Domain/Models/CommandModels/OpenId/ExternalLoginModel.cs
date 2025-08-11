// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.OpenId
{
    using System.ComponentModel.DataAnnotations;
    using System.Security.Claims;
    using Fsel.Identity.Domain.Constants;

    public class ExternalLoginModel : IRequestBodyTenantAware
    {
        [Required(ErrorMessage = "i18n_Email_cannot_be_empty")]
        [MaxLength(254, ErrorMessage = "i18n_limit_number_characters")]
        [RegularExpression(RegexSettings.EmailValid, ErrorMessage = "i18n_Email_is_not_valid")]
        public string? Email { get; set; }

        public bool IsEmailReadonly { get; set; } = true;

        [RegularExpression(Common.Helpers.RegexHelper.PhoneNumberValid, ErrorMessage = "i18n_number_phone_invalid")]
        [Required(ErrorMessage = "i18n_Phone_number_cannot_empty")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "i18n_First_name_cannot_be_empty")]
        [MaxLength(50, ErrorMessage = "i18n_limit_number_characters")]
        [RegularExpression(RegexSettings.FullNameValid, ErrorMessage = "i18n_First_name_is_not_valid")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "i18n_Last_name_cannot_be_empty")]
        [MaxLength(50, ErrorMessage = "i18n_limit_number_characters")]
        [RegularExpression(RegexSettings.FullNameValid, ErrorMessage = "i18n_Last_name_is_not_valid")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "i18n_Birth_day_cannot_be_empty")]
        public DateTime? Birthday { get; set; }

        public string? Provider { get; set; }

        public ClaimsPrincipal? Principal { get; set; }

        public string? ReturnUrl { get; set; }

        public string? Identity
        {
            get { return Email; }
            set { Email = value; }
        }

        public string? UserName
        {
            get { return Email; }
            set { Email = value; }
        }

        public Guid? UserId { get; set; }
    }
}
